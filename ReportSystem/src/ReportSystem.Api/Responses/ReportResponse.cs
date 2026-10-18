namespace ReportSystem.Api.Responses;

public record ReportResponse(
    Guid Id,
    string Name,
    string Status,
    DateTime CreatedAtUtc,
    string? DownloadUrl,
    string? FailureReason);