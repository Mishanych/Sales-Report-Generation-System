using System.Text.Json;
using Microsoft.Extensions.Options;
using ReportSystem.Application.Abstractions;
using ReportSystem.Application.Models;
using ReportSystem.Infrastructure.Messaging.Options;

namespace ReportSystem.Infrastructure.Messaging.Handlers;

public class ReportGenerationFailedEventHandler : IReportStatusEventHandler
{
    private readonly IReportRepository _repository;
    public string RoutingKey { get; }

    public ReportGenerationFailedEventHandler(IReportRepository repository, IOptions<RabbitMqOptions> options)
    {
        _repository = repository;
        RoutingKey = options.Value.ReportFailedRoutingKey;
    }
    public async Task HandleAsync(string jsonPayload, CancellationToken cancellationToken)
    {
        var reportEvent = JsonSerializer.Deserialize<ReportGenerationFailed>(jsonPayload);
        if (reportEvent == null) return;

        var report = await _repository.GetReportByIdAsync(reportEvent.ReportId, cancellationToken);
        if (report != null)
        {
            report.MarkAsFailed(reportEvent.ErrorMessage);
            await _repository.UpdateReportAsync(report, cancellationToken);
        }
    }
}