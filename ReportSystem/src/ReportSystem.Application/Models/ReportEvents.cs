namespace ReportSystem.Application.Models;

public record ReportGenerationStarted(Guid ReportId, DateTime ReportStartedOn);
public record ReportGenerationCompleted(Guid ReportId, string Filename, DateTime ReportCompletedOn);
public record ReportGenerationFailed(Guid ReportId, string ErrorMessage, DateTime ReportFailedOn);