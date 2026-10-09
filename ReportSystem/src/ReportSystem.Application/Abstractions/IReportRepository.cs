using ReportSystem.Domain.Entities;

namespace ReportSystem.Application.Abstractions;

public interface IReportRepository
{
    Task AddReportAsync(Report report, CancellationToken cancellationToken = default);
    Task UpdateReportAsync(Report report,  CancellationToken cancellationToken = default);
    Task<Report?> GetReportByIdAsync(Guid id, CancellationToken cancellationToken = default);
}