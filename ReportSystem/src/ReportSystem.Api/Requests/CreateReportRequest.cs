namespace ReportSystem.Api.Requests;

public record CreateReportRequest(
    string Name,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    List<CreateReportItemRequest?>? Items
);