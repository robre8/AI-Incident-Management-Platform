using IncidentPlatform.API.Services;
using IncidentPlatform.Application.Interfaces;
using IncidentPlatform.Application.Services;
using IncidentPlatform.Infrastructure.Data;
using IncidentPlatform.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<IncidentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var mongoSettings = builder.Configuration
    .GetSection("MongoSettings")
    .Get<MongoSettings>() ?? throw new InvalidOperationException("MongoSettings not configured.");

builder.Services.AddSingleton(mongoSettings);
builder.Services.AddSingleton<MongoLogContext>();

builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
builder.Services.AddScoped<IIncidentService, IncidentService>();
builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IAIAnalysisService, AIAnalysisService>();
builder.Services.AddScoped<IAwsFileService, AwsFileService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-apply pending EF Core migrations (safe to run on every startup)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IncidentDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();