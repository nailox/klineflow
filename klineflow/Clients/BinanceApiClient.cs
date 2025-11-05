using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using klineflow.Config;
using System.Collections.Generic;

namespace klineflow.Clients
{
 public class BinanceApiClient
 {
 private readonly HttpClient _http;
 private readonly BinanceSettings _settings;

 public BinanceApiClient(HttpClient http, IOptions<BinanceSettings> options)
 {
 _http = http;
 _settings = options.Value;
 }

 public async Task<List<List<object>>> GetKlinesAsync(string symbol, string interval, int limit =500)
 {
 var url = $"{_settings.BaseUrl}/api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}";
 var response = await _http.GetFromJsonAsync<List<List<object>>>(url);
 return response ?? new List<List<object>>();
 }
 }
}