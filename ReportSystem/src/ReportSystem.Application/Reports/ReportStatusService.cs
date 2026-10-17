using ReportSystem.Application.Abstractions;

namespace ReportSystem.Application.Reports;

public class ReportStatusService : IReportStatusService
{
    private readonly IReportRepository _repository;

    public ReportStatusService(IReportRepository repository)
    {
        _repository = repository;
    }

    public async Task MarkStartedAsync(Guid reportId, CancellationToken cancellationToken)
    {
        var report = await _repository.GetReportByIdAsync(reportId, cancellationToken);
        if (report != null)
        {
            report.MarkAsProcessing();
            await _repository.UpdateReportAsync(report, cancellationToken);
        }
        else
        {
            // TODO: add logger
        }
    }

    public async Task MarkFailedAsync(Guid reportId, string errorMessage, CancellationToken cancellationToken)
    {
        var report = await _repository.GetReportByIdAsync(reportId, cancellationToken);
        if (report != null)
        {
            report.MarkAsFailed(errorMessage);
            await _repository.UpdateReportAsync(report, cancellationToken);
        }
        else
        {
            // TODO: add logger
        }
    }

    public async Task MarkCompletedAsync(Guid reportId, string filename, CancellationToken cancellationToken)
    {
        var report = await _repository.GetReportByIdAsync(reportId, cancellationToken);
        if (report != null)
        {
            report.MarkAsCompleted(filename);
            await _repository.UpdateReportAsync(report, cancellationToken);
        }
        else
        {
            // TODO: add logger
        }
    }
}