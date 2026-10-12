using ReportSystem.Application.Models;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Application.Abstractions;

public interface IReportCalculationService
{
    decimal GetTotalRevenue(IEnumerable<ReportItem> items);
    int GetTotalUnitsSold(IEnumerable<ReportItem> items);
    List<PdfReportItemModel> GetReportItemsBreakdown(IEnumerable<ReportItem> items);
    List<PdfCategoryStatsModel> GetCategoryStats(IEnumerable<ReportItem> items);
}