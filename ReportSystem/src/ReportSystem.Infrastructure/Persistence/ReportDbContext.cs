using Microsoft.EntityFrameworkCore;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Persistence;

public class ReportDbContext : DbContext
{
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportItem> ReportItems => Set<ReportItem>();
    
    public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportDbContext).Assembly);
    }
}