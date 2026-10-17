using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ReportSystem.Contracts;
using ReportSystem.Infrastructure.Messaging.Options;

namespace ReportSystem.Infrastructure.Messaging.Publishers;

public class ReportStatusPublisher : IReportStatusPublisher
{
    private readonly IRabbitMqConnectionManager _connectionManager;
    private readonly RabbitMqOptions _options;

    public ReportStatusPublisher(IRabbitMqConnectionManager connectionManager, IOptions<RabbitMqOptions> options)
    {
        _connectionManager = connectionManager;
        _options = options.Value;
    }
    
    public Task PublishStartedAsync(Guid reportId, CancellationToken cancellationToken)
        => PublishAsync(_options.ReportStartedRoutingKey, new ReportGenerationStarted(reportId, DateTime.UtcNow), cancellationToken);

    public Task PublishCompletedAsync(Guid reportId, string filename, CancellationToken cancellationToken)
        => PublishAsync(_options.ReportCompletedRoutingKey, new ReportGenerationCompleted(reportId, filename, DateTime.UtcNow), cancellationToken);

    public Task PublishFailedAsync(Guid reportId, string errorMessage, CancellationToken cancellationToken)
        => PublishAsync(_options.ReportFailedRoutingKey, new ReportGenerationFailed(reportId, errorMessage, DateTime.UtcNow), cancellationToken);
    
    private async Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken)
    {
        var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: _options.ReportsExchange,
            routingKey: routingKey,
            body: body,
            cancellationToken: cancellationToken);
    }
}