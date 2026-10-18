using Microsoft.AspNetCore.Mvc;
using ReportSystem.Api.Requests;
using ReportSystem.Api.Responses;
using ReportSystem.Application.Models;
using ReportSystem.Application.Reports;
using ReportSystem.Domain.Enums;

namespace ReportSystem.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private const int MaxStringLength = 200;
    
    private readonly string CreateReportStatusUrl = "/api/reports/{0}";
    private readonly CreateReportService _createReportService;
    private readonly GetReportService _getReportService;

    public ReportsController(CreateReportService createReportService, GetReportService getReportService)
    {
        _createReportService = createReportService;
        _getReportService = getReportService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateReportRequest request,
        CancellationToken cancellationToken)
    {
        List<CreateReportItemInput> items = new();

        if (!IsReportRequestValid(request))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Wrong report name, period start/end date or items list is empty");
        }
        
        
        foreach (var item in request.Items)
        {
            if (Enum.TryParse<Category>(item.Category, true, out var category)
                && Enum.IsDefined(category)
                && category != Category.None)
            {
                if (string.IsNullOrWhiteSpace(item.ProductName) || item.ProductName.Length > MaxStringLength)
                {
                    return Problem(statusCode: StatusCodes.Status400BadRequest,
                        detail: $"ProductName can't be empty and greater than {MaxStringLength} characters for item: " +
                                item.ProductName);
                }

                if (item.Quantity <= 0)
                {
                    return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Quantity should be greater than 0 for item: " + item.ProductName);
                }
                
                if (item.UnitPrice < 0)
                {
                    return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "UnitPrice is less than 0 for item: " + item.ProductName);
                }
                
                if (!string.Equals(item.Currency, "EUR",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        detail: "Only EUR currency is supported.");
                }
                
                items.Add(new CreateReportItemInput(item.ProductName, category, item.Quantity, "EUR", item.UnitPrice));
            }
            else
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Wrong category for item: " + item.ProductName);
            }
        }

        CreateReportInput reportInput = new CreateReportInput(request.Name, request.PeriodStart, request.PeriodEnd, items);
        var reportId = await _createReportService.CreateReport(reportInput, cancellationToken);

        var statusUrl = string.Format(CreateReportStatusUrl, reportId);
        return Accepted(statusUrl, new
        {
            reportId,
            status = nameof(ReportStatus.Queued),
            statusUrl
        });
    }

    private static bool IsReportRequestValid(CreateReportRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.Name) && request.Name.Length <= MaxStringLength &&
               request.PeriodStart <= request.PeriodEnd && request.Items is { Count: > 0 } &&
               request.Items.All(item => item is not null);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Guid  can't be empty");
        }
        var report = await _getReportService.GetByIdAsync(id, cancellationToken);

        if (report is null)
        {
            return NotFound();
        }

        var response = new ReportResponse(
            report.Id,
            report.Name,
            report.Status.ToString(),
            report.CreatedAtUtc,
            report.Status == ReportStatus.Completed
                ? $"/api/reports/{report.Id}/file"
                : null,
            report.Status == ReportStatus.Failed
                ? report.FailureReason
                : null);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllReports([FromQuery(Name = "status")] ReportStatus? reportStatus, CancellationToken cancellationToken)
    {
        if (reportStatus.HasValue &&
            !Enum.IsDefined(reportStatus.Value))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Unknown report status.");
        }
        
        var reports = reportStatus is null
            ? await _getReportService.GetAllReports(cancellationToken)
            : await _getReportService.GetAllReportsByStatus(
                reportStatus.Value, cancellationToken);
        
        var response = reports.Select(report => new ReportSummaryResponse(
            report.Id,
            report.Name,
            report.Status.ToString(),
            report.CreatedAtUtc));

        return Ok(response);
    }
    
    [HttpGet("{id}/file")]
    public async Task<IActionResult> DownloadFile([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Guid  can't be empty");
        }

        var report = await _getReportService.GetByIdAsync(id, cancellationToken);

        if (report == null)
        {
            return NotFound();
        }

        if (report.Status != ReportStatus.Completed)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Report is not ready",
                detail: $"The report is currently {report.Status}.");
        }
        
        if (string.IsNullOrWhiteSpace(report.ReportFileName))
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "The report file is unavailable.");
        }

        byte[] fileData = await _getReportService.GetFileAsync(report.ReportFileName, cancellationToken);
        
        return File(fileData,  "application/pdf", report.ReportFileName);
    }
}