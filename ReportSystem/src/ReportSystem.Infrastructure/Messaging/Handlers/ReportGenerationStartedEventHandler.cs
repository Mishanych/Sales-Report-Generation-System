using System.Text.Json;
using Microsoft.Extensions.Options;
using ReportSystem.Application.Abstractions;
using ReportSystem.Contracts;
using ReportSystem.Infrastructure.Messaging.Options;

namespace ReportSystem.Infrastructure.Messaging.Handlers;

public class ReportGenerationStartedEventHandler : IReportStatusEventHandler
{
    private readonly IReportStatusService _reportStatusService;
    public string RoutingKey { get; }

    public ReportGenerationStartedEventHandler(IReportStatusService reportStatusService, IOptions<RabbitMqOptions> options)
    {
        _reportStatusService = reportStatusService;
        RoutingKey = options.Value.ReportStartedRoutingKey;
    }
    
    public async Task HandleAsync(string jsonPayload, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<ReportGenerationStarted>(jsonPayload);
        if (message == null) return;

        await _reportStatusService.MarkStartedAsync(message.ReportId, cancellationToken);
    }
}