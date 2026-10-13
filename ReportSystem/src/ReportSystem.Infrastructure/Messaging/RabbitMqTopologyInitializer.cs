using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ReportSystem.Infrastructure.Messaging.Options;

namespace ReportSystem.Infrastructure.Messaging;

public class RabbitMqTopologyInitializer : IHostedService
{
    private readonly IRabbitMqConnectionManager _connectionManager;
    private readonly RabbitMqOptions _options;

    public RabbitMqTopologyInitializer(
        IRabbitMqConnectionManager connectionManager, 
        IOptions<RabbitMqOptions> options)
    {
        _connectionManager = connectionManager;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(_options.ReportsExchange, ExchangeType.Topic, cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(_options.ReportGenerationQueue, durable: true, exclusive: false,
            autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(_options.ReportGenerationResultsQueue, durable: true, exclusive: false,
            autoDelete: false, cancellationToken: cancellationToken);

        await channel.QueueBindAsync(_options.ReportGenerationQueue, _options.ReportsExchange, _options.ReportRequestedRoutingKey,
            cancellationToken: cancellationToken);
        await channel.QueueBindAsync(_options.ReportGenerationResultsQueue, _options.ReportsExchange, _options.ReportResultsRoutingKeyPattern,
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}