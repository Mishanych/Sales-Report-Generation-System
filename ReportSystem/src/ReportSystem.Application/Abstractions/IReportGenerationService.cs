using ReportSystem.Contracts;

namespace ReportSystem.Application.Abstractions;

public interface IReportGenerationService
{
    Task<string> GenerateAsync(GenerateReportRequested request, CancellationToken cancellationToken);
}