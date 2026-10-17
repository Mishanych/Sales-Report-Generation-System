namespace ReportSystem.Api.Requests;

public record CreateReportItemRequest(
    string ProductName,
    string Category,
    int Quantity,
    string Currency,
    decimal UnitPrice
);