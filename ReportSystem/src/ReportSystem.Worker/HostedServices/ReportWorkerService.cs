using Microsoft.Extensions.Options;
using ReportSystem.Infrastructure.Messaging;
using ReportSystem.Infrastructure.Messaging.Options;
using ReportSystem.Worker.Consumers;

namespace ReportSystem.Worker.HostedService;

public class ReportWorkerService : BackgroundService
{
    private readonly IRabbitMqConnectionManager _connectionManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<RabbitMqOptions> _options;

    public ReportWorkerService(IRabbitMqConnectionManager connectionManager, IServiceProvider serviceProvider,
        IOptions<RabbitMqOptions> options)
    {
        _connectionManager = connectionManager;
        _serviceProvider = serviceProvider;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection= await _connectionManager.GetConnectionAsync(stoppingToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        
        try
        {
            await using var consumer = new RabbitReportMessageConsumer(channel, _serviceProvider);
            
            await channel.BasicConsumeAsync(
                queue: _options.Value.ReportGenerationQueue,
                autoAck: false,
                consumerTag: string.Empty,
                noLocal: false,
                exclusive: false,
                arguments: null,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            await channel.DisposeAsync();
        }
    }
}