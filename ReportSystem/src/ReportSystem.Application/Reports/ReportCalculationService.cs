using ReportSystem.Application.Abstractions;
using ReportSystem.Application.Models;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Application.Reports;

public class ReportCalculationService : IReportCalculationService
{
    public ReportCalculatedData GetReportCalculatedData(IReadOnlyCollection<ReportItemDto> items)
    {
        return new ReportCalculatedData(
            GetTotalUnitsSold(items),
            GetTotalRevenue(items),
            GetReportItemsBreakdown(items),
            GetCategoryStats(items));
    }

    public decimal GetTotalRevenue(IEnumerable<ReportItemDto> items)
    {
        decimal revenue = 0;

        foreach (ReportItemDto item in items)
        {
            revenue += item.UnitPrice * item.Quantity;
        }
        
        return revenue;
    }
    
    public int GetTotalUnitsSold(IEnumerable<ReportItemDto> items)
    {
        int quantityOfUnits = 0;

        foreach (ReportItemDto item in items)
        {
            quantityOfUnits += item.Quantity;
        }
        
        return quantityOfUnits;
    }
    
    public List<PdfReportItemModel> GetReportItemsBreakdown(IEnumerable<ReportItemDto> items)
    {
        return items.Select(item => new PdfReportItemModel
        {
            ProductName = item.Name,
            CategoryName = item.Category.ToString(),
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.UnitPrice * item.Quantity,
            Currency = item.Currency
        }).ToList();
    }
    
    public List<PdfCategoryStatsModel> GetCategoryStats(IEnumerable<ReportItemDto> items)
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