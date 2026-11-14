using System.Text.Json;
using CryptoIntelligence.Api.Application.Interfaces;
using CryptoIntelligence.Api.Application.Services;
using CryptoIntelligence.Api.Infrastructure.ClickHouse;
using CryptoIntelligence.Api.Infrastructure.Repositories;
using CryptoIntelligence.Api.Middleware;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ClickHouseOptions>(builder.Configuration.GetSection(ClickHouseOptions.SectionName));
builder.Services.AddHttpClient<IClickHouseClient, ClickHouseHttpClient>();

builder.Services.AddScoped<ICandleRepository, CandleRepository>();
builder.Services.AddScoped<IMarketDataRepository, MarketDataRepository>();
builder.Services.AddScoped<IIndicatorRepository, IndicatorRepository>();
builder.Services.AddScoped<ISignalRepository, SignalRepository>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<INewsImpactRepository, NewsImpactRepository>();
builder.Services.AddScoped<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<ICandleService, CandleService>();
builder.Services.AddScoped<IMarketDataService, MarketDataService>();
builder.Services.AddScoped<IIndicatorService, IndicatorService>();
builder.Services.AddScoped<ISignalService, SignalService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<INewsImpactService, NewsImpactService>();
builder.Services.AddScoped<IStatusService, StatusService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Crypto Intelligence System API",
        Version = "v1",
        Description = "API truy vấn dữ liệu market, nến, chỉ báo và tín hiệu từ ClickHouse."
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy => policy
        .WithOrigins("http://localhost:3000", "http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Crypto Intelligence API v1");
    options.RoutePrefix = "swagger";
});
app.UseCors("frontend");
app.MapControllers();
app.Run();

public partial class Program { }
