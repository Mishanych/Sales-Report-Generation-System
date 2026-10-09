using ReportSystem.Application.Abstractions;

namespace ReportSystem.Infrastructure.Storage;

public class LocalReportFileStorage : IReportFileStorage
{
    private readonly string _storagePath;
    
    public LocalReportFileStorage(IConfiguration configuration)
    {
        var relativePath = configuration["FileStorage:Path"] ?? "storage/reports";
        _storagePath = Path.Combine(AppContext.BaseDirectory, relativePath);
        
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }
    
    public async Task SaveAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(fileName);
        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);
    }

    public bool Exists(string fileName)
    {
        var fullPath = GetFullPath(fileName);
        return File.Exists(fullPath);
    }

    public async Task<byte[]> ReadAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(fileName);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Report file '{fileName}' was not found.", fullPath);
        }

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }
    
    private string GetFullPath(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName); 
        return Path.Combine(_storagePath, safeFileName);
    }
}