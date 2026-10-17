using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReportSystem.Application.Abstractions;
using ReportSystem.Application.Models;
using Document = QuestPDF.Fluent.Document;

namespace ReportSystem.Infrastructure.Pdf;

public class PdfReportGenerator : IPdfReportGenerator
{
    public PdfReportGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// e.g. - Mechanical Keyboard: 5 × EUR 80.00 = EUR 400.00
    /// </summary>
    private const string ProductBreakdown = "-  {0}: {1} × {2} = {3}";
    /// <summary>
    /// e.g.  - Accessories: EUR 400.00
    /// </summary>
    private const string CategoryBreakdown = "-  {0}: {1}";

    public async Task<byte[]> GenerateReportAsync(PdfReportModel reportModel,
        CancellationToken cancellationToken = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text(reportModel.ReportName)
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    column.Spacing(15);

                    column.Item().Text($"Period: {reportModel.ReportPeriod}").Bold();
                    column.Item().Text($"Total units sold: {reportModel.TotalUnitsSold}").Bold();
                    column.Item().Text($"Total revenue: {reportModel.TotalRevenue}").Bold();

                    column.Item().Column(descriptor =>
                    {
                        descriptor.Spacing(8);
                        descriptor.Item().Text("Revenue by category:").Bold().FontSize(14);

                        descriptor.Item().PaddingLeft(10).Column(list =>
                        {
                            list.Spacing(4);
                            foreach (var categoryStatsModel in reportModel.CategoryStats)
                            {
                                var itemText = string.Format(CategoryBreakdown, categoryStatsModel.CategoryName,
                                    categoryStatsModel.FormattedTotalPrice);
                                list.Item().Text(itemText);
                            }
                        });
                    });

                    column.Item().Column(sec2 =>
                    {
                        sec2.Spacing(8);
                        sec2.Item().Text("Products:").Bold().FontSize(14);

                        sec2.Item().PaddingLeft(10).Column(list =>
                        {
                            list.Spacing(4);
                            foreach (var reportItemModel in reportModel.Items)
                            {
                                var itemText = string.Format(ProductBreakdown, reportItemModel.ProductName,
                                    reportItemModel.Quantity, reportItemModel.FormattedUnitPrice,
                                    reportItemModel.FormattedTotalPrice);
                                list.Item().Text(itemText);
                            }
                        });
                    });
                });
            });
        });
        
        return await Task.Run(document.GeneratePdf, cancellationToken);
    }
}