using System.Globalization;
using ReportSystem.Application.Reports;

namespace ReportSystem.Application.Models;

public class PdfReportModel
{
    private const string DateFormat = "dd MMM yyyy";
    
    public string ReportName { get; init; }
    public string ReportPeriod { get; init; }
    public int TotalUnitsSold { get; init; }
    public decimal TotalRevenue { get; init; }
    
    public IReadOnlyCollection<PdfReportItemModel> Items { get; init; }
    public IReadOnlyCollection<PdfCategoryStatsModel> CategoryStats { get; init; }

    public PdfReportModel(string reportName, DateTime periodStart, DateTime periodEnd, ReportCalculatedData calculatedData)
    {
        ReportName = reportName;
        ReportPeriod = FormatPeriod(periodStart, periodEnd);
        TotalUnitsSold = calculatedData.TotalUnitsSold;
        TotalRevenue = calculatedData.TotalRevenue;

        Items = calculatedData.ItemsBreakdown;
        CategoryStats = calculatedData.CategoryStats;
    }

    private string FormatPeriod(DateTime startPeriod, DateTime endPeriod)
    {
        string periodStartFormatted = startPeriod.ToString(DateFormat, CultureInfo.InvariantCulture);
        string periodEndFormatted = endPeriod.ToString(DateFormat, CultureInfo.InvariantCulture);
        
        return $"{periodStartFormatted} - {periodEndFormatted}";
    }
}