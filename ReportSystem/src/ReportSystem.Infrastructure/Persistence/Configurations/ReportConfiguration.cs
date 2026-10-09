using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");
        builder.HasKey(x => x.Id);
        
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.PeriodStart)
            .IsRequired();
        builder.Property(r => r.PeriodEnd)
            .IsRequired();
        
        builder.Property(r=>r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.HasMany(r=>r.Items)
            .WithOne()
            .HasForeignKey("ReportId")
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Navigation(r => r.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}