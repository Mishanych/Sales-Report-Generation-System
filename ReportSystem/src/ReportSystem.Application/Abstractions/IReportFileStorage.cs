namespace ReportSystem.Application.Abstractions;

public interface IReportFileStorage
{
    Task SaveAsync(string fileName, string content, CancellationToken cancellationToken = default);
    Task ExistsAsync(string fileName, CancellationToken cancellationToken = default);
    Task ReadAsync(string fileName, CancellationToken cancellationToken = default);
}