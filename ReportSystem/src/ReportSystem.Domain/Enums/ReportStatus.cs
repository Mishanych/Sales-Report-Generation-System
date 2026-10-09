namespace ReportSystem.Domain.Enums;

public enum ReportStatus
{
    None = 0,
    Queued,
    Processing,
    Completed,
    Failed,
}