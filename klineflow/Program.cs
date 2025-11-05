using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using klineflow.Config;
using klineflow.Clients;
using klineflow.Services;
using klineflow.Repositories;
using klineflow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<BinanceSettings>(builder.Configuration.GetSection("Binance"));

// HttpClient for Binance
builder.Services.AddHttpClient<BinanceApiClient>();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI
builder.Services.AddScoped<BinanceService>();
builder.Services.AddScoped<TimeSeriesService>();
builder.Services.AddSingleton<ForecastService>();
builder.Services.AddScoped<ICandleRepository, CandleRepository>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
 app.UseSwagger();
 app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
