using ReportSystem.Application.Abstractions;
using ReportSystem.Application.Models;
using ReportSystem.Contracts;

namespace ReportSystem.Application.Generation;

public class ReportGenerationService : IReportGenerationService
{
    private readonly IReportCalculationService _calculationService;
    private readonly IPdfReportGenerator _pdfReportGenerator;
    private readonly IReportFileStorage _reportFileStorage;

    public ReportGenerationService(IReportCalculationService calculationService, IPdfReportGenerator pdfReportGenerator,
        IReportFileStorage reportFileStorage)
    {
        _calculationService = calculationService;
        _pdfReportGenerator = pdfReportGenerator;
        _reportFileStorage = reportFileStorage;
    }

    public async Task<string> GenerateAsync(GenerateReportRequested request, CancellationToken cancellationToken)
    {
        var reportCalculatedData = _calculationService.GetReportCalculatedData(request.Items);

        PdfReportModel pdfReportModel = new PdfReportModel(
            request.ReportName,
            request.ReportStartDate,
            request.ReportEndDate,
            reportCalculatedData);

        byte[] pdfData = await _pdfReportGenerator.GenerateReportAsync(pdfReportModel, cancellationToken);
        string fileName = $"{request.ReportId}.pdf";
        await _reportFileStorage.SaveAsync(fileName, pdfData, cancellationToken);
        
        return fileName;
    }
}