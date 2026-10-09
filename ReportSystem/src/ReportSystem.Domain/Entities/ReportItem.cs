using ReportSystem.Domain.Enums;

namespace ReportSystem.Domain.Entities;

public class ReportItem
{
    public Guid Id { get; private set; }
    public string ProductName { get; private set; }
    public Category Category { get; private set; }
    public int Quantity { get; private set; }
    public string Currency { get; private set; }
    public decimal UnitPrice { get; private set; }

    public ReportItem(Guid id, string productName, Category category, int quantity, string currency, decimal unitPrice)
    {
        Id = id;
        ProductName = productName;
        Category = category;
        Quantity = quantity;
        Currency = currency;
        UnitPrice = unitPrice;
    }
}