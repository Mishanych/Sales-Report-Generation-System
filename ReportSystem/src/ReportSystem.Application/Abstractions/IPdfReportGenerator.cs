using ReportSystem.Application.Models;

namespace ReportSystem.Application.Abstractions;

public interface IPdfReportGenerator
{
    Task<byte[]> GenerateReportAsync(PdfReportModel reportModel, CancellationToken cancellationToken = default);
}