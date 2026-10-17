using System.Text;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReportSystem.Infrastructure.Messaging.Handlers;

namespace ReportSystem.Infrastructure.Messaging;

public class ReportStatusConsumer : AsyncEventingBasicConsumer, IAsyncDisposable
{
    private readonly IServiceProvider _serviceProvider;

    public ReportStatusConsumer(IChannel channel, IServiceProvider serviceProvider) : base(channel)
    {
        _serviceProvider = serviceProvider;
        ReceivedAsync += OnMessageReceivedAsync;
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs deliverEventArgs)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var cancellationToken = cts.Token;
        
        using var scope = _serviceProvider.CreateScope();
        
        try
        {
            var body = deliverEventArgs.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var routingKey = deliverEventArgs.RoutingKey;

            var handlers = scope.ServiceProvider.GetServices<IReportStatusEventHandler>();
            var handler = handlers.FirstOrDefault(h => h.RoutingKey.Equals(routingKey, StringComparison.OrdinalIgnoreCase));

            if (handler != null)
            {
                await handler.HandleAsync(json, cts.Token);
            }

            await Channel.BasicAckAsync(deliveryTag: deliverEventArgs.DeliveryTag, multiple: false,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
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