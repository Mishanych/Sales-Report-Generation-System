using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReportSystem.Application.Abstractions;
using ReportSystem.Contracts;
using ReportSystem.Infrastructure.Messaging.Constants;
using ReportSystem.Infrastructure.Messaging.Options;
using ReportSystem.Infrastructure.Messaging.Publishers;

namespace ReportSystem.Worker.Consumers;

public class RabbitReportMessageConsumer : AsyncEventingBasicConsumer, IAsyncDisposable
{
    private const int MaxRetryAttempts = 3;
    private const string DeathHeader = "x-death";
    private const string DeathQueueField = "queue";
    private const string DeathReasonField = "reason";
    private const string DeathCountField = "count";
    private const string RejectedReason = "rejected";
    
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMqOptions _options;

    public RabbitReportMessageConsumer(IChannel channel, IServiceProvider serviceProvider, IOptions<RabbitMqOptions> options) : base(channel)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        ReceivedAsync += OnMessageReceivedAsync;
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs deliverEventArgs)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var cancellationToken = cts.Token;

        using var scope = _serviceProvider.CreateScope();
        var statusPublisher = scope.ServiceProvider.GetRequiredService<IReportStatusPublisher>();
        var reportGenerationService = scope.ServiceProvider.GetRequiredService<IReportGenerationService>();

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

            string fileName = await reportGenerationService.GenerateAsync(message, cancellationToken);
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
        if (headers != null && headers.TryGetValue(DeathHeader, out var xDeath) && xDeath is IList<object> deaths && deaths.Count > 0)
        {
            foreach (var value in deaths)
            {
                if (value is not IDictionary<string, object> entry)
                    continue;

                entry.TryGetValue(DeathQueueField, out var queueValue);
                entry.TryGetValue(DeathReasonField, out var reasonValue);
                entry.TryGetValue(DeathCountField, out var countValue);

                if (ReadText(queueValue) == _options.ReportGenerationQueue &&
                    ReadText(reasonValue) == RejectedReason &&
                    countValue is not null)
                {
                    return Convert.ToInt32(countValue);
                }
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
    
    private static string? ReadText(object? value) => value switch
    {
        byte[] bytes => Encoding.UTF8.GetString(bytes),
        string text => text,
        _ => null
    };
}