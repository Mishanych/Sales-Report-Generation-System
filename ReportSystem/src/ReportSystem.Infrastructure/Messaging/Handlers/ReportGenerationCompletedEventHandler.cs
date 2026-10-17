using System.Text.Json;
using Microsoft.Extensions.Options;
using ReportSystem.Application.Abstractions;
using ReportSystem.Contracts;
using ReportSystem.Infrastructure.Messaging.Options;

namespace ReportSystem.Infrastructure.Messaging.Handlers;

public class ReportGenerationCompletedEventHandler : IReportStatusEventHandler
{
    private readonly IReportStatusService _reportStatusService;
    public string RoutingKey { get; }

    public ReportGenerationCompletedEventHandler(IReportStatusService reportStatusService, IOptions<RabbitMqOptions> options)
    {
        _reportStatusService = reportStatusService;
        RoutingKey = options.Value.ReportCompletedRoutingKey;
    }
    
    public async Task HandleAsync(string jsonPayload, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<ReportGenerationCompleted>(jsonPayload);
        if (message == null) return;
        
        await _reportStatusService.MarkCompletedAsync(
            message.ReportId,
            message.Filename,
            cancellationToken);
    }
}