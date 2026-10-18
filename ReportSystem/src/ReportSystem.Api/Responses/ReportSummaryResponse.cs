namespace ReportSystem.Api.Responses;

public record ReportSummaryResponse(
    Guid Id,
    string Name,
    string Status,
    DateTime CreatedAtUtc);