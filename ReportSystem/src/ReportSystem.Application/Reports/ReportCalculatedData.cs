using ReportSystem.Application.Models;

namespace ReportSystem.Application.Reports;

public record ReportCalculatedData(
    int TotalUnitsSold,
    decimal TotalRevenue,
    IReadOnlyCollection<PdfReportItemModel> ItemsBreakdown,
    IReadOnlyCollection<PdfCategoryStatsModel> CategoryStats);