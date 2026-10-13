namespace ReportSystem.Infrastructure.Messaging.Options;

public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public string ReportsExchange { get; set; } = "reports";
    public string ReportGenerationQueue { get; set; } = "report-generation-worker";
    public string ReportGenerationResultsQueue { get; set; } = "report-generation-api-results";
    
    public string ReportRequestedRoutingKey { get; set; } = "report.generation.requested";
    public string ReportStartedRoutingKey { get; set; } = "report.generation.started";
    public string ReportCompletedRoutingKey { get; set; } = "report.generation.completed";
    public string ReportFailedRoutingKey { get; set; } = "report.generation.failed";
    public string ReportResultsRoutingKeyPattern { get; set; } = "report.generation.*";
}