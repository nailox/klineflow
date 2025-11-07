using klineflow.Clients;
using klineflow.Config;
using klineflow.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;

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

// Token service (generates JWT tokens)
builder.Services.AddSingleton<TokenService>();

// JWT Authentication setup
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyString = jwtSection.GetValue<string>("Key");
var issuer = jwtSection.GetValue<string>("Issuer");
var audience = jwtSection.GetValue<string>("Audience");

if (string.IsNullOrEmpty(keyString))
{
 throw new ArgumentException("JWT Key is not configured. Set 'Jwt:Key' to a32+ byte value (preferably base64).\nExample (powershell): $k=[Convert]::ToBase64String((1..32|%{Get-Random -Maximum256})); echo $k\nThen set this value in configuration or as an environment variable.");
}

byte[] keyBytes;
try
{
 // Try base64 decode first (recommended). If it fails, fall back to UTF8 bytes.
 keyBytes = Convert.FromBase64String(keyString);
}
catch
{
 keyBytes = Encoding.UTF8.GetBytes(keyString);
}

// Validate minimum key length for HS256 (256 bits =32 bytes)
if (keyBytes.Length <32)
{
 throw new ArgumentException($"JWT Key is too short ({keyBytes.Length} bytes). HS256 requires at least256 bits (32 bytes).\nProvide a key of at least32 bytes. You can generate one with: openssl rand -base6432");
}

var signingKey = new SymmetricSecurityKey(keyBytes);

builder.Services.AddAuthentication(options =>
{
 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
 options.RequireHttpsMetadata = false;
 options.SaveToken = true;
 options.TokenValidationParameters = new TokenValidationParameters
 {
 ValidateIssuer = true,
 ValidateAudience = true,
 ValidateIssuerSigningKey = true,
 ValidIssuer = issuer,
 ValidAudience = audience,
 IssuerSigningKey = signingKey
 };
});

builder.Services.AddAuthorization(options =>
{
 options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddControllers();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
