using Microsoft.Extensions.Options;
using ReportSystem.Infrastructure.Messaging;
using ReportSystem.Infrastructure.Messaging.Options;
using ReportSystem.Worker.Consumers;

namespace ReportSystem.Worker;

/*public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}*/
public class Worker : BackgroundService
{
    private readonly IRabbitMqConnectionManager _connectionManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<RabbitMqOptions> _options;

    public Worker(IRabbitMqConnectionManager connectionManager, IServiceProvider serviceProvider,
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
            await using var consumer = new RabbitReportMessageConsumer(channel, _serviceProvider, _options);
            
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
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            await channel.DisposeAsync();
        }
    }
}