using Microsoft.EntityFrameworkCore;
using ReportSystem.Api.ErrorHandling;
using ReportSystem.Application.Abstractions;
using ReportSystem.Application.Reports;
using ReportSystem.Infrastructure.Messaging;
using ReportSystem.Infrastructure.Messaging.Handlers;
using ReportSystem.Infrastructure.Messaging.Options;
using ReportSystem.Infrastructure.Messaging.Publishers;
using ReportSystem.Infrastructure.Persistence;
using ReportSystem.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSingleton<IRabbitMqConnectionManager, RabbitMqConnectionManager>();

builder.Services.AddTransient<CreateReportService>();
builder.Services.AddTransient<GetReportService>();

builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportMessagePublisher, ReportRequestPublisher>();
builder.Services.AddScoped<IReportFileStorage, LocalReportFileStorage>();


// report status
builder.Services.AddScoped<IReportStatusService, ReportStatusService>();
builder.Services.AddScoped<IReportStatusEventHandler,
    ReportGenerationStartedEventHandler>();
builder.Services.AddScoped<IReportStatusEventHandler,
    ReportGenerationCompletedEventHandler>();
builder.Services.AddScoped<IReportStatusEventHandler,
    ReportGenerationFailedEventHandler>();


var reportsConnectionString = builder.Configuration.GetConnectionString("ReportsDatabase");
if (string.IsNullOrWhiteSpace(reportsConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:ReportsDatabase must be configured.");
}

builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseSqlServer(reportsConnectionString));

builder.Services.AddHostedService<RabbitMqTopologyInitializer>();
builder.Services.AddHostedService<ReportStatusConsumerService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "Report System API v1"));
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapControllers();
app.Run();
