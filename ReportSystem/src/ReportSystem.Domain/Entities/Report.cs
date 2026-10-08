using ReportSystem.Domain.Enums;

namespace ReportSystem.Domain.Entities;

public class Report
{
    public string Name { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<ReportItem> Items { get; set; }
    
    public ReportStatus Status => _status;
    private ReportStatus _status;

    public void MarkAsProcessing()
    {
        _status = ReportStatus.Processing;
    }

    public void MarkAsCompleted()
    {
        _status = ReportStatus.Completed;
    }

    public void MarkAsFailed()
    {
        _status = ReportStatus.Failed;
    }
}