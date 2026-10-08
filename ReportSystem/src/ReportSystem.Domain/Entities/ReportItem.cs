using ReportSystem.Domain.Enums;

namespace ReportSystem.Domain.Entities;

public class ReportItem
{
    public string Name { get; set; }
    public Category Category { get; set; }
    public float Quantity { get; set; }
    public float Price { get; set; }
}