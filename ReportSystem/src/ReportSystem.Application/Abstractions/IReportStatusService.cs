namespace ReportSystem.Application.Abstractions;

public interface IReportStatusService
{
    Task MarkStartedAsync(Guid reportId, CancellationToken cancellationToken);
    Task MarkFailedAsync(Guid reportId, string errorMessage, CancellationToken cancellationToken);
    Task MarkCompletedAsync(Guid reportId, string filename, CancellationToken cancellationToken);
}