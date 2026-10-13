using ReportSystem.Application.Reports;

namespace ReportSystem.Worker.Publishers;

public interface IReportStatusPublisher
{
    Task PublishStartedAsync(Guid reportId, CancellationToken cancellationToken);
    Task PublishFailedAsync(Guid reportId, string errorMessage, CancellationToken cancellationToken);
    Task PublishCompletedAsync(Guid reportId, string filename, CancellationToken cancellationToken);
}