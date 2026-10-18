using ReportSystem.Application.Abstractions;
using ReportSystem.Application.Generation;
using ReportSystem.Application.Reports;
using ReportSystem.Infrastructure.Messaging;
using ReportSystem.Infrastructure.Messaging.Options;
using ReportSystem.Infrastructure.Messaging.Publishers;
using ReportSystem.Infrastructure.Pdf;
using ReportSystem.Infrastructure.Storage;
using ReportSystem.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSingleton<IRabbitMqConnectionManager, RabbitMqConnectionManager>();
builder.Services.AddScoped<IReportGenerationService, ReportGenerationService>();
builder.Services.AddScoped<IReportCalculationService, ReportCalculationService>();
builder.Services.AddScoped<IPdfReportGenerator, PdfReportGenerator>();
builder.Services.AddScoped<IReportFileStorage, LocalReportFileStorage>();
builder.Services.AddScoped<IReportStatusPublisher, ReportStatusPublisher>();

builder.Services.AddHostedService<RabbitMqTopologyInitializer>();
builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();