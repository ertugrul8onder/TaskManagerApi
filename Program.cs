using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks; // Add this using
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks; // Add this using
using Serilog;
using System.Net.Mime; // Add this using for MediaTypeNames
using System.Text.Json; // Add this using for JsonSerializer
using TaskManagerApi.Middleware;
using TaskManagerApi.Services;
using TaskManagerApi.Validation;
using Models;

// Configure Serilog logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/taskmanagerapi-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog(); // Use Serilog for logging

    // CORS politikasını ekliyoruz
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins(
                    "http://localhost:5173",  // Development için
                    "https://taskmanagerweb.ertugrulonder.com" // Production için
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Entity Framework için SQLite bağlantısı ekliyoruz
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlite("Data Source=tasks.db"));

// Controller'ları ekliyoruz
builder.Services.AddControllers();

    // Register FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();

    // Register TaskService
    builder.Services.AddScoped<ITaskService, TaskService>();

    // Register AutoMapper
    builder.Services.AddAutoMapper(typeof(Program)); // Or typeof(TaskProfile)

    // Register Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<TaskDbContext>("Database", HealthStatus.Unhealthy, tags: new[] { "db" });


    var app = builder.Build();

// In Development, we apply migrations automatically.
// For Production environments, database migrations should be handled via
// deployment scripts or CI/CD pipelines using 'dotnet ef database update'
// or other database deployment tools. Automatic migration at startup is
// generally discouraged in production to avoid unintended schema changes
// or startup failures.
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
        try
        {
            db.Database.Migrate(); // Apply migrations in Development
            // Optional: Seed data here if needed for development
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating or seeding the database in Development.");
            // Depending on policy, you might want to re-throw or handle
        }
    }
}

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

// Add global error handling middleware
app.UseGlobalExceptionHandler();

// CORS middleware'ini ekliyoruz
app.UseCors("AllowReactApp");

// Map Health Checks Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var result = JsonSerializer.Serialize(
            new {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration
                }),
                totalDuration = report.TotalDuration
            });
        context.Response.ContentType = MediaTypeNames.Application.Json;
        await context.Response.WriteAsync(result);
    }
});

app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
