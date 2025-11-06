using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System;
using klineflow.Models;
using System.Linq;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Net;

namespace klineflow.Clients
{
    public class SupabaseRestClient
    {
        private readonly HttpClient _http;
        private readonly string _url;
        private readonly string _apikey;
        private readonly ILogger<SupabaseRestClient> _logger;

        public SupabaseRestClient(HttpClient http, IConfiguration config, ILogger<SupabaseRestClient> logger)
        {
            _http = http;
            // expect Supabase:Url and Supabase:Key in configuration
            _url = config["Supabase:Url"] ?? throw new ArgumentNullException("Supabase:Url");
            _apikey = config["Supabase:Key"] ?? throw new ArgumentNullException("Supabase:Key");
            _logger = logger;

            _http.DefaultRequestHeaders.Remove("apikey");
            _http.DefaultRequestHeaders.Remove("Authorization");
            _http.DefaultRequestHeaders.Add("apikey", _apikey);
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apikey}");

            _logger.LogInformation("Supabase client configured for {Url} with apiKey={ApiKeyMask}", _url, MaskKey(_apikey));
        }

        public async Task InsertCandlesAsync(List<Candle> candles)
        {
            var baseUrl = _url.TrimEnd('/');

            // Build payload WITHOUT Id to avoid inserting into identity column
            var payload = candles.Select(c => new
            {
                symbol = c.Symbol,
                opentime = c.OpenTime,
                open = c.Open,
                high = c.High,
                low = c.Low,
                close = c.Close,
                volume = c.Volume,
                closetime = c.CloseTime,
                quoteassetvolume = c.QuoteAssetVolume,
                numberoftrades = c.NumberOfTrades,
                takerbuybaseassetvolume = c.TakerBuyBaseAssetVolume,
                takerbuyquoteassetvolume = c.TakerBuyQuoteAssetVolume
            }).ToList();

            var json = JsonSerializer.Serialize(payload);

            // Single lowercase endpoint for the candles table
            var endpoint = $"{baseUrl}/rest/v1/candles";

            _logger.LogInformation("Supabase insert POST {Endpoint} - payload count: {Count} - apiKey={ApiKeyMask}", endpoint, payload.Count, MaskKey(_apikey));

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Prefer", "return=representation");

            var res = await _http.SendAsync(request);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                _logger.LogError("Supabase insert to {Endpoint} failed: {Status} {Reason} - {Body}", endpoint, (int)res.StatusCode, res.ReasonPhrase, body);
                throw new HttpRequestException($"Supabase insert failed: {(int)res.StatusCode} {res.ReasonPhrase} - {body}");
            }

            _logger.LogInformation("Supabase insert succeeded to {Endpoint}: {Status}", endpoint, (int)res.StatusCode);
        }

        public async Task DeleteAllAsync()
        {
            var baseUrl = _url.TrimEnd('/');
            // PostgREST requires a WHERE clause for DELETE; use id=gt.0 to match all rows
            var endpoint = $"{baseUrl}/rest/v1/candles?id=gt.0";

            _logger.LogInformation("Supabase DELETE all POST {Endpoint} - apiKey={ApiKeyMask}", endpoint, MaskKey(_apikey));

            var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            // Prefer header can request returning representation; not needed for delete-only
            request.Headers.Add("Prefer", "return=representation");

            var res = await _http.SendAsync(request);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                _logger.LogError("Supabase delete all failed: {Status} {Reason} - {Body}", (int)res.StatusCode, res.ReasonPhrase, body);
                throw new HttpRequestException($"Supabase delete all failed: {(int)res.StatusCode} {res.ReasonPhrase} - {body}");
            }

            _logger.LogInformation("Supabase delete all succeeded: {Status}", (int)res.StatusCode);
        }

        public async Task<List<Candle>> GetRecentAsync(string symbol, int take = 100)
        {
            var baseUrl = _url.TrimEnd('/');
            var encoded = WebUtility.UrlEncode(symbol);

            // Single lowercase endpoint for the candles table
            var endpoint = $"{baseUrl}/rest/v1/candles?select=*&symbol=eq.{encoded}&order=opentime.desc&limit={take}";
            _logger.LogInformation("Supabase GET {Endpoint} - apiKey={ApiKeyMask}", endpoint, MaskKey(_apikey));

            var res = await _http.GetAsync(endpoint);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                _logger.LogError("Supabase GET to {Endpoint} failed: {Status} {Reason} - {Body}", endpoint, (int)res.StatusCode, res.ReasonPhrase, body);
                throw new HttpRequestException($"Supabase query failed: {(int)res.StatusCode} {res.ReasonPhrase} - {body}");
            }

            var list = await res.Content.ReadFromJsonAsync<List<Candle>>();
            return list ?? new List<Candle>();
        }

        // New: get candles in a time range (opentime in milliseconds since epoch)
        public async Task<List<Candle>> GetRangeAsync(string symbol, long? startOpenTimeMs, long? endOpenTimeMs, int limit = 100)
        {
            var baseUrl = _url.TrimEnd('/');
            var encoded = WebUtility.UrlEncode(symbol);
            var filters = new List<string> { $"symbol=eq.{encoded}" };
            if (startOpenTimeMs.HasValue) filters.Add($"opentime=gte.{startOpenTimeMs.Value}");
            if (endOpenTimeMs.HasValue) filters.Add($"opentime=lte.{endOpenTimeMs.Value}");
            var filterQuery = string.Join("&", filters);
            var endpoint = $"{baseUrl}/rest/v1/candles?select=*&{filterQuery}&order=opentime.desc&limit={limit}";

            _logger.LogInformation("Supabase RANGE GET {Endpoint} - apiKey={ApiKeyMask}", endpoint, MaskKey(_apikey));

            var res = await _http.GetAsync(endpoint);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                _logger.LogError("Supabase RANGE GET to {Endpoint} failed: {Status} {Reason} - {Body}", endpoint, (int)res.StatusCode, res.ReasonPhrase, body);
                throw new HttpRequestException($"Supabase range query failed: {(int)res.StatusCode} {res.ReasonPhrase} - {body}");
            }

            var list = await res.Content.ReadFromJsonAsync<List<Candle>>();
            return list ?? new List<Candle>();
        }

        private static string MaskKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return "<empty>";
            if (key.Length <= 8) return new string('*', key.Length);
            return $"{key.Substring(0, 4)}…{key.Substring(key.Length - 4)}";
        }
    }
}
