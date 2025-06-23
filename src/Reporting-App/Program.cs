using Reporting_App.Components;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Database;
using Reporting_App.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' nicht gefunden.");

builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register custom services
builder.Services.AddScoped<IReportingService, ReportingService>();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();