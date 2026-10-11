using System.Globalization;
using ReportSystem.Domain.Entities;
using ReportSystem.Domain.Enums;

namespace ReportSystem.Application.Models;

public class PdfReportModel
{
    private const string DateFormat = "dd MMM yyyy";
    
    public string ReportName { get; init; }
    public string ReportPeriod { get; init; }
    public int TotalUnitsSold { get; init; }
    public decimal TotalRevenue { get; init; }
    
    public IReadOnlyList<PdfReportItemModel> Items { get; init; }
    public IReadOnlyList<PdfCategoryStatsModel> CategoryStats { get; init; }
    
    public PdfReportModel(Report report, int totalUnitsSold, decimal totalRevenue, List<PdfReportItemModel> items, List<PdfCategoryStatsModel>  categoryStats)
    {
        ReportName = report.Name;
        ReportPeriod = FormatPeriod(report.PeriodStart, report.PeriodEnd);
        TotalUnitsSold = totalUnitsSold;
        TotalRevenue = totalRevenue;
        
        Items = items;
        CategoryStats = categoryStats;
    }

    private string FormatPeriod(DateTime startPeriod, DateTime endPeriod)
    {
        string periodStartFormatted = startPeriod.ToString(DateFormat, CultureInfo.InvariantCulture);
        string periodEndFormatted = endPeriod.ToString(DateFormat, CultureInfo.InvariantCulture);
        
        return $"{periodStartFormatted} - {periodEndFormatted}";
    }
}