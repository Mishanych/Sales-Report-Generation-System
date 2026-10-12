using ReportSystem.Application.Abstractions;
using ReportSystem.Application.Models;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Application.Reports;

public class ReportCalculationService : IReportCalculationService
{
    public decimal GetTotalRevenue(IEnumerable<ReportItem> items)
    {
        decimal revenue = 0;

        foreach (ReportItem item in items)
        {
            revenue += item.UnitPrice * item.Quantity;
        }
        
        return revenue;
    }
    
    public int GetTotalUnitsSold(IEnumerable<ReportItem> items)
    {
        int quantityOfUnits = 0;

        foreach (ReportItem item in items)
        {
            quantityOfUnits += item.Quantity;
        }
        
        return quantityOfUnits;
    }
    
    public List<PdfReportItemModel> GetReportItemsBreakdown(IEnumerable<ReportItem> items)
    {
        return items.Select(item => new PdfReportItemModel
        {
            ProductName = item.ProductName,
            CategoryName = item.Category.ToString(),
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.UnitPrice * item.Quantity,
            Currency = item.Currency
        }).ToList();
    }
    
    public List<PdfCategoryStatsModel> GetCategoryStats(IEnumerable<ReportItem> items)
    {
        return items
            .GroupBy(item => item.Category)
            .Select(group => 
            {
                var totalRevenue = group.Sum(x => x.Quantity * x.UnitPrice);
            
                return new PdfCategoryStatsModel
                {
                    CategoryName = group.Key.ToString(),
                    TotalRevenue = totalRevenue,
                };
            })
            .ToList();
    }
}