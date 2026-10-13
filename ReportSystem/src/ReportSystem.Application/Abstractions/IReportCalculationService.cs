using ReportSystem.Application.Models;
using ReportSystem.Application.Reports;

namespace ReportSystem.Application.Abstractions;

public interface IReportCalculationService
{
    ReportCalculatedData GetReportCalculatedData(IReadOnlyCollection<ReportItemDto> items);
}