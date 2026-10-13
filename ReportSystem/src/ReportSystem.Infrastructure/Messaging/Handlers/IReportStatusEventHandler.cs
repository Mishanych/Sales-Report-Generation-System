namespace ReportSystem.Infrastructure.Messaging.Handlers;

public interface IReportStatusEventHandler
{
    string RoutingKey { get; }
    Task HandleAsync(string jsonPayload, CancellationToken cancellationToken);
}