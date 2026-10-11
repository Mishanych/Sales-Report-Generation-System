namespace ReportSystem.Infrastructure.Messaging.Options;

public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public string ReportGenerationQueue { get; set; } = "report-generation-queue";
}