using ReportSystem.Application.Abstractions;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Pdf;

public class PdfReportGenerator : IReportGenerator
{
    public Task GenerateReportAsync(Report report, CancellationToken cancellationToken = default)
    {
        
    }
}