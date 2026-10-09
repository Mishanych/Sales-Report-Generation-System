using System.Text;
using ReportSystem.Domain.Entities;
using ReportSystem.Domain.Enums;

namespace ReportSystem.Application.Reports;

public class ReportCalculationService
{
    /// <summary>
    /// e.g. Mechanical Keyboard: 5 × EUR 80.00 = EUR 400.00
    /// </summary>
    private const string ProductBreakdown = "{0}: {1} x {2} {3} = {2} {4}";
    
    public decimal GetTotalRevenue(IEnumerable<ReportItem> items)
    {
        decimal revenue = 0;

        foreach (ReportItem item in items)
        {
            revenue += item.UnitPrice * item.Quantity;
        }
        
        return revenue;
    }
    
    public decimal GetTotalUnitsSold(IEnumerable<ReportItem> items)
    {
        decimal quantityOfUnits = 0;

        foreach (ReportItem item in items)
        {
            quantityOfUnits += item.Quantity;
        }
        
        return quantityOfUnits;
    }
    
    public decimal GetRevenueByCategory(IEnumerable<ReportItem> items, Category category)
    {
        var filteredItems = items.Where(i => i.Category == category);
        return GetTotalRevenue(filteredItems);
    }
    
    public string GetReportItemsBreakdown(IEnumerable<ReportItem> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Products:");
        
        foreach (ReportItem item in items)
        {
            decimal totalPrice = item.UnitPrice * item.Quantity;
            sb.AppendFormat(ProductBreakdown, item.ProductName, item.Quantity, item.Currency, item.UnitPrice, totalPrice);
        }
        
        return sb.ToString();
    }
}