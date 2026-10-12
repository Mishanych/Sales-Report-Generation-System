namespace ReportSystem.Application.Models;

public record GenerateReportRequested(
    Guid ReportId,
    DateTime RequestedAt);