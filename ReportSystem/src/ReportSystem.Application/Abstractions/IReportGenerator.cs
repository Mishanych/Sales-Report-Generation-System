using ReportSystem.Application.Models;

namespace ReportSystem.Application.Abstractions;

public interface IReportGenerator
{
    Task<byte[]> GenerateReportAsync(PdfReportModel reportModel, CancellationToken cancellationToken = default);
}