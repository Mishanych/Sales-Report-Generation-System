using System.Text.Json;
using Microsoft.Extensions.Options;
using ReportSystem.Application.Abstractions;
using ReportSystem.Application.Models;
using ReportSystem.Infrastructure.Messaging.Options;

namespace ReportSystem.Infrastructure.Messaging.Handlers;

public class ReportGenerationStartedEventHandler : IReportStatusEventHandler
{
    private readonly IReportRepository _repository;
    public string RoutingKey { get; }

    public ReportGenerationStartedEventHandler(IReportRepository repository, IOptions<RabbitMqOptions> options)
    {
        _repository = repository;
        RoutingKey = options.Value.ReportStartedRoutingKey;
    }
    
    public async Task HandleAsync(string jsonPayload, CancellationToken cancellationToken)
    {
        var reportEvent = JsonSerializer.Deserialize<ReportGenerationStarted>(jsonPayload);
        if (reportEvent == null) return;

        var report = await _repository.GetReportByIdAsync(reportEvent.ReportId, cancellationToken);
        if (report != null)
        {
            report.MarkAsProcessing();
            await _repository.UpdateReportAsync(report, cancellationToken);
        }
    }
}