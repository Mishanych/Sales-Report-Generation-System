using ReportSystem.Api.Contracts.Requests;
using ReportSystem.Application.Abstractions;
using ReportSystem.Application.Models;
using ReportSystem.Domain.Entities;
using ReportSystem.Domain.Enums;

namespace ReportSystem.Application.Reports;

public class CreateReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IReportMessagePublisher _reportMessagePublisher;

    public CreateReportService(IReportRepository  reportRepository, IReportMessagePublisher  reportMessagePublisher)
    {
        _reportRepository = reportRepository;
        _reportMessagePublisher = reportMessagePublisher;
    }
    
    public async Task CreateReport(CreateReportRequest request, CancellationToken cancellationToken =  default)
    {
        var report = new Report(
            Guid.NewGuid(),
            request.Name,
            request.PeriodStart,
            request.PeriodEnd,
            string.Empty,
            request.Items.Select(item =>
                {
                    var category = Enum.TryParse<Category>(item.Category, ignoreCase: true, out var parsedCategory)
                        ? parsedCategory
                        : Category.None;

                    return new ReportItem(
                        Guid.NewGuid(),
                        item.ProductName,
                        category,
                        item.Quantity,
                        item.Currency,
                        item.UnitPrice);
                }
            ).ToList()
        );
        
        await _reportRepository.AddReportAsync(report, cancellationToken);

        var reportItemDtos = report.Items
            .Select(item => new ReportItemDto(
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.Category.ToString(),
                item.Currency))
            .ToList();
        
        var message = new GenerateReportRequested(
            report.Id,
            report.Name,
            report.PeriodStart,
            report.PeriodEnd,
            reportItemDtos,
            DateTime.UtcNow);
        await _reportMessagePublisher.PublishReportRequestedAsync(message, cancellationToken);
    }
}