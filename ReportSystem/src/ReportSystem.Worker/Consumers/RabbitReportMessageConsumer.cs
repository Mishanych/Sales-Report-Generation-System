using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReportSystem.Application.Abstractions;
using ReportSystem.Application.Models;
using ReportSystem.Infrastructure.Messaging.Constants;
using ReportSystem.Worker.Publishers;

namespace ReportSystem.Worker.Consumers;

public class RabbitReportMessageConsumer : AsyncEventingBasicConsumer, IAsyncDisposable
{
    private const int MaxRetryAttempts = 3;
    private readonly IServiceProvider _serviceProvider;

    public RabbitReportMessageConsumer(IChannel channel, IServiceProvider serviceProvider) : base(channel)
    {
        _serviceProvider = serviceProvider;
        ReceivedAsync += OnMessageReceivedAsync;
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs deliverEventArgs)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var cancellationToken = cts.Token;

        using var scope = _serviceProvider.CreateScope();
        var statusPublisher = scope.ServiceProvider.GetRequiredService<IReportStatusPublisher>();

        Guid currentReportId = TryExtractReportId(deliverEventArgs.BasicProperties.Headers);
        int retryCount = GetRetryCount(deliverEventArgs.BasicProperties.Headers);

        try
        {
            var body = deliverEventArgs.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<GenerateReportRequested>(json);

            if (message == null)
            {
                await HandleFatalErrorAsync(statusPublisher, currentReportId, "Deserialized message is null.",
                    deliverEventArgs);
                return;
            }

            currentReportId = message.ReportId;
            if (retryCount == 0)
            {
                await statusPublisher.PublishStartedAsync(currentReportId, cancellationToken);
            }

            string fileName = await CreateReport(scope, message, cancellationToken);
            await statusPublisher.PublishCompletedAsync(message.ReportId, fileName, cancellationToken);

            await Channel.BasicAckAsync(deliveryTag: deliverEventArgs.DeliveryTag, multiple: false,
                cancellationToken: CancellationToken.None);
        }
        catch (JsonException ex)
        {
            string message = $"Invalid message payload format: {ex.Message}";
            await HandleFatalErrorAsync(statusPublisher, currentReportId, message, deliverEventArgs);
        }
        catch (InvalidOperationException ex)
        {
            await HandleFatalErrorAsync(statusPublisher, currentReportId, ex.Message, deliverEventArgs);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or TimeoutException)
        {
            if (retryCount < MaxRetryAttempts)
            {
                await Channel.BasicNackAsync(deliverEventArgs.DeliveryTag, multiple: false, requeue: false,
                    cancellationToken: CancellationToken.None);
            }
            else
            {
                string message = $"Exceeded max retries. Last error: {ex.Message}";
                await HandleFatalErrorAsync(statusPublisher, currentReportId, message, deliverEventArgs);
            }
        }
        catch (Exception ex)
        {
            string message = $"Unhandled exception: {ex.Message}";
            await HandleFatalErrorAsync(statusPublisher, currentReportId, message, deliverEventArgs);
        }
    }

    private async Task<string> CreateReport(IServiceScope scope, GenerateReportRequested message, CancellationToken cancellationToken)
    {
        var calculationService = scope.ServiceProvider.GetRequiredService<IReportCalculationService>();
        var pdfReportGenerator = scope.ServiceProvider.GetRequiredService<IReportGenerator>();
        var reportFileStorage = scope.ServiceProvider.GetRequiredService<IReportFileStorage>();
        
        var reportCalculatedData = calculationService.GetReportCalculatedData(message.Items);

        PdfReportModel pdfReportModel = new PdfReportModel(
            message.ReportName,
            message.ReportStartDate,
            message.ReportEndDate,
            reportCalculatedData);

        byte[] pdfData = await pdfReportGenerator.GenerateReportAsync(pdfReportModel, cancellationToken);
        string fileName = $"{message.ReportId}.pdf";
        await reportFileStorage.SaveAsync(fileName, pdfData, cancellationToken);
        
        return fileName;
    }

    public ValueTask DisposeAsync()
    {
        ReceivedAsync -= OnMessageReceivedAsync;
        return ValueTask.CompletedTask;
    }
    
    private static Guid TryExtractReportId(IDictionary<string, object?>? headers)
    {
        if (headers != null && 
            headers.TryGetValue(RabbitMqHeaders.ReportId, out var idObj) && 
            idObj is byte[] bytes)
        {
            var idStr = Encoding.UTF8.GetString(bytes);
            if (Guid.TryParse(idStr, out var reportId))
            {
                return reportId;
            }
        }
        return Guid.Empty;
    }
    
    private int GetRetryCount(IDictionary<string, object?>? headers)
    {
        if (headers != null && headers.TryGetValue("x-death", out var xDeath) && xDeath is IList<object> list && list.Count > 0)
        {
            if (list[0] is IDictionary<string, object> entry && entry.TryGetValue("count", out var count))
            {
                return Convert.ToInt32(count);
            }
        }
        return 0;
    }
    
    private async Task HandleFatalErrorAsync(
        IReportStatusPublisher statusPublisher, 
        Guid reportId, 
        string errorReason, 
        BasicDeliverEventArgs args)
    {
        if (reportId != Guid.Empty)
        {
            await statusPublisher.PublishFailedAsync(reportId, errorReason, CancellationToken.None);
        }
    
        await Channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: CancellationToken.None);
    }
}