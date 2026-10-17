using ReportSystem.Domain.Enums;

namespace ReportSystem.Application.Models;

public record CreateReportItemInput(
    string ProductName,
    Category Category,
    int Quantity,
    string Currency,
    decimal UnitPrice);