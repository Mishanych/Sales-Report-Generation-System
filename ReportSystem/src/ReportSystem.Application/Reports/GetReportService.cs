using ReportSystem.Application.Abstractions;
using ReportSystem.Domain.Entities;
using ReportSystem.Domain.Enums;

namespace ReportSystem.Application.Reports;

public class GetReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IReportFileStorage _reportFileStorage;

    public GetReportService(IReportRepository reportRepository, IReportFileStorage reportFileStorage)
    {
        _reportRepository = reportRepository;
        _reportFileStorage = reportFileStorage;
    }

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _reportRepository.GetReportByIdAsync(id, cancellationToken);
    }

    public async Task<List<Report>> GetAllReports(CancellationToken cancellationToken)
    {
        return await _reportRepository.GetAllReports(cancellationToken);
    }

    public async Task<List<Report>> GetAllReportsByStatus(ReportStatus reportStatus, CancellationToken cancellationToken)
    {
        return await _reportRepository.GetAllReportsByStatus(reportStatus, cancellationToken);
    }

    public async Task<byte[]> GetFileAsync(string fileName, CancellationToken cancellationToken)
    {
        return await _reportFileStorage.ReadAsync(fileName, cancellationToken);
    }
}