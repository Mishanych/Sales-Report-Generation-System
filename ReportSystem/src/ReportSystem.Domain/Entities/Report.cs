using ReportSystem.Domain.Enums;

namespace ReportSystem.Domain.Entities;

public class Report
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public ReportStatus Status { get; private set; }
    public string ReportFileName { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    
    public IReadOnlyCollection<ReportItem> Items => _items.AsReadOnly();
    private readonly List<ReportItem> _items;
    
    public Report(Guid id, string name, DateTime periodStart, DateTime periodEnd, string reportFileName, List<ReportItem> items)
    {
        Id = id;
        Name = name;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        ReportFileName = reportFileName;
        _items = items;
        Status = ReportStatus.Queued;
        CreatedAtUtc = DateTime.UtcNow;
    }
    
    private Report()
    {
        Name = string.Empty;
        ReportFileName = string.Empty;
        _items = new List<ReportItem>();
    }

    public void MarkAsProcessing()
    {
        FailureReason = null;
        Status = ReportStatus.Processing;
    }

    public void MarkAsCompleted(string filename)
    {
        ReportFileName = filename;
        Status = ReportStatus.Completed;
    }

    public void MarkAsFailed(string errorMessage)
    {
        FailureReason = errorMessage;
        Status = ReportStatus.Failed;
    }
}