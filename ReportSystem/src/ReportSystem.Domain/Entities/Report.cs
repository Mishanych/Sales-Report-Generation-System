using ReportSystem.Domain.Enums;

namespace ReportSystem.Domain.Entities;

public class Report
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public ReportStatus Status { get; private set; }
    
    public IReadOnlyCollection<ReportItem> Items => _items.AsReadOnly();
    private readonly List<ReportItem> _items;
    
    public Report(){ }
    
    public Report(Guid id, string name, DateTime periodStart, DateTime periodEnd, List<ReportItem> items)
    {
        Id = id;
        Name = name;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        _items = items;
        Status = ReportStatus.Queued;
    }

    public void MarkAsProcessing()
    {
        Status = ReportStatus.Processing;
    }

    public void MarkAsCompleted()
    {
        Status = ReportStatus.Completed;
    }

    public void MarkAsFailed()
    {
        Status = ReportStatus.Failed;
    }
}