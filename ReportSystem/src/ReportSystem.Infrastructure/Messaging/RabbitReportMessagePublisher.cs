using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ReportSystem.Application.Abstractions;
using ReportSystem.Application.Models;
using ReportSystem.Infrastructure.Messaging.Options;

namespace ReportSystem.Infrastructure.Messaging;

public class RabbitReportMessagePublisher : IReportMessagePublisher
{
    private readonly IRabbitMqConnectionManager _connectionManager;
    private readonly IOptions<RabbitMqOptions> _options;

    public RabbitReportMessagePublisher(IRabbitMqConnectionManager connectionManager, IOptions<RabbitMqOptions> options)
    {
        _connectionManager = connectionManager;
        _options = options;
    }
    
    public async Task PublishReportRequestedAsync(GenerateReportRequested message, CancellationToken cancellationToken = default)
    {
        var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _options.Value.ReportGenerationQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _options.Value.ReportGenerationQueue,
            body: body,
            cancellationToken: cancellationToken);
    }
}