namespace ReportSystem.Application.Models;

public record CreateReportInput(
    string Name,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    List<CreateReportItemInput> Items);