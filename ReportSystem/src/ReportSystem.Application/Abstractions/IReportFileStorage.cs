namespace ReportSystem.Application.Abstractions;

public interface IReportFileStorage
{
    Task SaveAsync(string fileName, byte[] content, CancellationToken cancellationToken = default);
    bool Exists(string fileName);
    Task<byte[]> ReadAsync(string fileName, CancellationToken cancellationToken = default);
}