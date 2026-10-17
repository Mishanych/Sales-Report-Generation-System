namespace ReportSystem.Contracts;

public record GenerateReportRequested(
    Guid ReportId,
    string ReportName,
    DateTime ReportStartDate,
    DateTime ReportEndDate,
    IReadOnlyCollection<ReportItemDto> Items,
    DateTime RequestedAt);
    
public record ReportItemDto(
    string Name,
    int Quantity,
    decimal UnitPrice,
    string Category,
    string Currency);