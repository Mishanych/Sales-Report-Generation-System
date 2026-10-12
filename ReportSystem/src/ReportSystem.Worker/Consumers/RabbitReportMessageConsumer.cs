using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReportSystem.Application.Abstractions;
using ReportSystem.Application.Models;
using ReportSystem.Domain.Entities;

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
        var reportRepository = scope.ServiceProvider.GetRequiredService<IReportRepository>();
        var calculationService = scope.ServiceProvider.GetRequiredService<IReportCalculationService>();
        var pdfReportGenerator = scope.ServiceProvider.GetRequiredService<IReportGenerator>();
        
        try
        {
            var body = deliverEventArgs.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<GenerateReportRequested>(json);

            if (message != null)
            {
                Report? report = await reportRepository.GetReportByIdAsync(message.ReportId, cancellationToken);
                if (report != null)
                {
                    PdfReportModel pdfReportModel = new PdfReportModel(
                        report,
                        calculationService.GetTotalUnitsSold(report.Items),
                        calculationService.GetTotalRevenue(report.Items),
                        calculationService.GetReportItemsBreakdown(report.Items),
                        calculationService.GetCategoryStats(report.Items));
                    
                    await pdfReportGenerator.GenerateReportAsync(pdfReportModel, cancellationToken);
                }
            }

            await Channel.BasicAckAsync(deliveryTag: deliverEventArgs.DeliveryTag, multiple: false,
                cancellationToken: cancellationToken);
        }
        catch (JsonException)
        {
            // issue with data
            await Channel.BasicNackAsync(
                deliveryTag: deliverEventArgs.DeliveryTag, 
                multiple: false, 
                requeue: false,
                cancellationToken: CancellationToken.None);
        }
        catch (HttpRequestException)
        {
            await Channel.BasicNackAsync(
                deliveryTag: deliverEventArgs.DeliveryTag, 
                multiple: false, 
                requeue: true,
                cancellationToken: CancellationToken.None);
        }
        catch (Exception)
        {
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