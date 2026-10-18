using ReportSystem.Domain.Entities;
using ReportSystem.Domain.Enums;

namespace ReportSystem.Application.Abstractions;

public interface IReportRepository
{
    Task AddReportAsync(Report report, CancellationToken cancellationToken = default);
    Task UpdateReportAsync(Report report,  CancellationToken cancellationToken = default);
    Task<Report?> GetReportByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Report>> GetAllReports(CancellationToken cancellationToken = default);
    Task<List<Report>> GetAllReportsByStatus(ReportStatus reportStatus, CancellationToken cancellationToken = default);
}