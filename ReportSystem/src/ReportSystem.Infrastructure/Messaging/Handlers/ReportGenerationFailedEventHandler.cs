using System.Text.Json;
using Microsoft.Extensions.Options;
using ReportSystem.Application.Abstractions;
using ReportSystem.Contracts;
using ReportSystem.Infrastructure.Messaging.Options;

namespace ReportSystem.Infrastructure.Messaging.Handlers;

public class ReportGenerationFailedEventHandler : IReportStatusEventHandler
{
    private readonly IReportStatusService _reportStatusService;
    public string RoutingKey { get; }

    public ReportGenerationFailedEventHandler(IReportStatusService reportStatusService, IOptions<RabbitMqOptions> options)
    {
        _reportStatusService = reportStatusService;
        RoutingKey = options.Value.ReportFailedRoutingKey;
    }
    public async Task HandleAsync(string jsonPayload, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<ReportGenerationFailed>(jsonPayload);
        if (message == null) return;

        await _reportStatusService.MarkFailedAsync(message.ReportId,  message.ErrorMessage, cancellationToken);
    }
}