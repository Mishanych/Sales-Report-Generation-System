using Microsoft.EntityFrameworkCore;
using ReportSystem.Application.Abstractions;
using ReportSystem.Domain.Entities;

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
}