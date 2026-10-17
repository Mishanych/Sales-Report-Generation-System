using ReportSystem.Application.Reports;
using ReportSystem.Contracts;

namespace ReportSystem.Application.Abstractions;

public interface IReportCalculationService
{
    ReportCalculatedData GetReportCalculatedData(IReadOnlyCollection<ReportItemDto> items);
}