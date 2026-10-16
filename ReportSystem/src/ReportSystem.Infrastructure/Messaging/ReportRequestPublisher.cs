using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ReportSystem.Application.Abstractions;
using ReportSystem.Application.Models;
using ReportSystem.Infrastructure.Messaging.Constants;
using ReportSystem.Infrastructure.Messaging.Options;

namespace ReportSystem.Infrastructure.Messaging;

public class ReportRequestPublisher : IReportMessagePublisher
{
    private readonly IRabbitMqConnectionManager _connectionManager;
    private readonly RabbitMqOptions _options;

    public ReportRequestPublisher(IRabbitMqConnectionManager connectionManager, IOptions<RabbitMqOptions> options)
    {
        _connectionManager = connectionManager;
        _options = options.Value;
    }
    
    public async Task PublishReportRequestedAsync(GenerateReportRequested message, CancellationToken cancellationToken = default)
    {
        var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        
        var properties = new BasicProperties
        {
            Headers = new Dictionary<string, object?>
            {
                { RabbitMqHeaders.ReportId, message.ReportId.ToString() }
            }
        };
        
        await channel.BasicPublishAsync(
            exchange: _options.ReportsExchange,
            routingKey: _options.ReportRequestedRoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}