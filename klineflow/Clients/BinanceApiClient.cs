using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using klineflow.Config;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace klineflow.Clients
{
 public class BinanceApiClient
 {
 private readonly HttpClient _http;
 private readonly BinanceSettings _settings;
 private readonly ILogger<BinanceApiClient> _logger;

 public BinanceApiClient(HttpClient http, IOptions<BinanceSettings> options, ILogger<BinanceApiClient> logger)
 {
 _http = http;
 _settings = options.Value;
 _logger = logger;
 }

 public async Task<List<JsonElement[]>> GetKlinesAsync(string symbol, string interval, int limit =500)
 {
 var url = $"{_settings.BaseUrl}/api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}";
 _logger.LogInformation("Binance URL: {Url}", url);

 var request = new HttpRequestMessage(HttpMethod.Get, url);
 // Set a Mozilla Firefox User-Agent header
 request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT10.0; Win64; x64; rv:117.0) Gecko/20100101 Firefox/117.0");

 var response = await _http.SendAsync(request);
 response.EnsureSuccessStatusCode();
 var stream = await response.Content.ReadAsStreamAsync();
 var items = await JsonSerializer.DeserializeAsync<List<JsonElement[]>>(stream);
 return items ?? new List<JsonElement[]>();
 }
 }
}