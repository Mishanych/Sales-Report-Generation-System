namespace ReportSystem.Application.Models;

public class PdfReportItemModel
{
    public string ProductName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
    public string Currency { get; init; } = string.Empty;

    public string FormattedUnitPrice => $"{Currency} {UnitPrice:N2}";
    public string FormattedTotalPrice => $"{Currency} {TotalPrice:N2}";
}