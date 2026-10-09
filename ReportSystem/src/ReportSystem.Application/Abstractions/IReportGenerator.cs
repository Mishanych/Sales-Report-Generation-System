using ReportSystem.Domain.Entities;

namespace ReportSystem.Application.Abstractions;

public interface IReportGenerator
{
    Task GenerateReportAsync(Report report, CancellationToken cancellationToken = default);
}