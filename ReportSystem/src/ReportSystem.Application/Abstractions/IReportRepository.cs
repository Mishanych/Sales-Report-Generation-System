using ReportSystem.Domain.Entities;

namespace ReportSystem.Application.Abstractions;

public interface IReportRepository
{
    void AddReport(Report report);
    void UpdateReport(Report report);
    Report GetReportById(string id);
}