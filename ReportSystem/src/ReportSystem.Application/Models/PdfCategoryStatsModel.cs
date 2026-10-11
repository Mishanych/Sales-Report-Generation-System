namespace ReportSystem.Application.Models;

public class PdfCategoryStatsModel
{
    public string CategoryName { get; init; } = string.Empty;
    public decimal TotalRevenue { get; init; }
    public string Currency { get; init; } = string.Empty;

    public string FormattedTotalPrice => $"{Currency} {TotalRevenue:N2}";
}