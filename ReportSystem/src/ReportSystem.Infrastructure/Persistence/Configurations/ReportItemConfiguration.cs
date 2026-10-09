using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Persistence.Configurations;

public class ReportItemConfiguration : IEntityTypeConfiguration<ReportItem>
{
    public void Configure(EntityTypeBuilder<ReportItem> builder)
    {
        builder.ToTable("ReportItems");
        builder.HasKey(i => i.Id);
        
        builder.Property(i=> i.ProductName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(i=> i.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("EUR");

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);
    }
}