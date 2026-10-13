using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReportSystem.Application.Abstractions;
using ReportSystem.Application.Models;
using ReportSystem.Worker.Publishers;

namespace ReportSystem.Worker.Consumers;

public class RabbitReportMessageConsumer : AsyncEventingBasicConsumer, IAsyncDisposable
{
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
        var calculationService = scope.ServiceProvider.GetRequiredService<IReportCalculationService>();
        var pdfReportGenerator = scope.ServiceProvider.GetRequiredService<IReportGenerator>();
        var reportFileStorage = scope.ServiceProvider.GetRequiredService<IReportFileStorage>();
        var statusPublisher = scope.ServiceProvider.GetRequiredService<IReportStatusPublisher>();
        
        Guid currentReportId = Guid.Empty;
        
        try
        {
            var body = deliverEventArgs.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<GenerateReportRequested>(json);

            if (message != null)
            {
                currentReportId = message.ReportId;
                await statusPublisher.PublishStartedAsync(currentReportId, cancellationToken);

                var reportCalculatedData = calculationService.GetReportCalculatedData(message.Items);

                PdfReportModel pdfReportModel = new PdfReportModel(
                    message.ReportName,
                    message.ReportStartDate,
                    message.ReportEndDate,
                    reportCalculatedData);
                    
                byte[] pdfData = await pdfReportGenerator.GenerateReportAsync(pdfReportModel, cancellationToken);
                string fileName = $"{currentReportId}.pdf";
                await reportFileStorage.SaveAsync(fileName, pdfData, cancellationToken);
                    
                await statusPublisher.PublishCompletedAsync(message.ReportId, fileName, cancellationToken);
            }

            await Channel.BasicAckAsync(deliveryTag: deliverEventArgs.DeliveryTag, multiple: false,
                cancellationToken: cancellationToken);
        }
        catch (JsonException ex)
        {
            if (currentReportId != Guid.Empty)
            {
                await statusPublisher.PublishFailedAsync(
                    currentReportId, 
                    $"Invalid message payload format: {ex.Message}", 
                    CancellationToken.None);
            }
            
            // issue with data
            await Channel.BasicNackAsync(
                deliveryTag: deliverEventArgs.DeliveryTag, 
                multiple: false, 
                requeue: false,
                cancellationToken: CancellationToken.None);
        }
        catch (Exception ex)
        {
            if (currentReportId != Guid.Empty)
            {
                await statusPublisher.PublishFailedAsync(currentReportId, ex.Message, CancellationToken.None);
            }
            
            await Channel.BasicNackAsync(
                deliveryTag: deliverEventArgs.DeliveryTag, 
                multiple: false, 
                requeue: !deliverEventArgs.Redelivered,
                cancellationToken: CancellationToken.None);
        }
    }
    
    public ValueTask DisposeAsync()
    {
        ReceivedAsync -= OnMessageReceivedAsync;
        return ValueTask.CompletedTask;
    }
}