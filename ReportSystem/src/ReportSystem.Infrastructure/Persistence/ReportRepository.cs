using Microsoft.EntityFrameworkCore;
using ReportSystem.Application.Abstractions;
using ReportSystem.Domain.Entities;
using ReportSystem.Domain.Enums;

namespace ReportSystem.Infrastructure.Persistence;

public class ReportRepository : IReportRepository
{
    private readonly ReportDbContext _dbContext;

    public ReportRepository(ReportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddReportAsync(Report report, CancellationToken cancellationToken = default)
    {
        await _dbContext.Reports.AddAsync(report, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateReportAsync(Report report, CancellationToken cancellationToken = default)
    {
        _dbContext.Reports.Update(report);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Report?> GetReportByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reports
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken: cancellationToken);
    }

    public async Task<List<Report>> GetAllReports(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reports
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Report>> GetAllReportsByStatus(ReportStatus reportStatus, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reports
            .Where(r => r.Status == reportStatus)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}