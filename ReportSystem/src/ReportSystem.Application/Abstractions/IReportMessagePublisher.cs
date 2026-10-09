using ReportSystem.Application.Models;

namespace ReportSystem.Application.Abstractions;

public interface IReportMessagePublisher
{
    Task PublishReportRequestedAsync(GenerateReportRequested message, CancellationToken cancellationToken = default);
}