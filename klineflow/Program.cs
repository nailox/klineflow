using klineflow.Clients;
using klineflow.Config;
using klineflow.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<BinanceSettings>(builder.Configuration.GetSection("Binance"));

// HttpClient for Binance
builder.Services.AddHttpClient<BinanceApiClient>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https://api.binance.com"); // Set the base address for Binance API
    });
    
// Supabase Rest client
builder.Services.AddHttpClient<SupabaseRestClient>();

// HttpClient for ForecastService (Gemini calls)
builder.Services.AddHttpClient<ForecastService>();

// DI - register services
builder.Services.AddScoped<BinanceService>();
builder.Services.AddScoped<TimeSeriesService>();
// ForecastService uses HttpClient via DI
builder.Services.AddScoped<ForecastService>();
builder.Services.AddScoped<SupabaseRestClient>();

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
