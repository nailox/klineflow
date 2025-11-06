using klineflow.DTOs;
using klineflow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;

namespace klineflow.Controllers
{
 [ApiController]
 [Route("api/[controller]")]
 public class CandlesController : ControllerBase
 {
 private readonly BinanceService _binance;
 private readonly TimeSeriesService _ts;
 private readonly ForecastService _forecast;

 public CandlesController(BinanceService binance, TimeSeriesService ts, ForecastService forecast)
 {
 _binance = binance;
 _ts = ts;
 _forecast = forecast;
 }

 [HttpPost("fetch")]
 public async Task<IActionResult> FetchAndStore([FromBody] CandlesRequestDto req)
 {
 var candles = await _binance.FetchCandlesAsync(req.Symbol, req.Interval, req.Limit);
 try
 {
 await _ts.StoreCandlesAsync(candles);
 return Ok(new { stored = candles.Count });
 }
 catch (HttpRequestException ex)
 {
 var msg = ex.Message ?? string.Empty;
 // detect common Postgres unique violation returned by Supabase
 if (msg.Contains("409") || msg.Contains("duplicate key") || msg.Contains("23505") || msg.Contains("already exists"))
 {
 return Conflict(new { error = "Value already exists" });
 }

 // otherwise return generic server error with message
 return StatusCode(500, new { error = "Insert failed", detail = ex.Message });
 }
 }

 [HttpPost("forecast")]
 public async Task<IActionResult> Forecast([FromBody] CandlesRequestDto req)
 {
 var candles = await _ts.GetRecentAsync(req.Symbol, req.Limit);
 // Call Gemini analysis with fixed prompt and candle data
 var geminiResponse = await _forecast.AnalyzeWithGeminiAsync(candles);
 return Ok(new { gemini = geminiResponse });
 }

 [HttpDelete("all")]
 [Authorize(Policy = "AdminOnly")]
 public async Task<IActionResult> DeleteAll()
 {
 await _ts.DeleteAllAsync();
 return Ok(new { deleted = true });
 }

 // GET api/timeseries/{symbol}/range?start=2025-01-01T00:00:00Z&end=2025-02-01T00:00:00Z
 [HttpGet("{symbol}/range")]
 public async Task<IActionResult> Range([FromRoute] string symbol, [FromQuery] string? start, [FromQuery] string? end, [FromQuery] int limit =100)
 {
 long? startMs = null;
 long? endMs = null;
 if (!string.IsNullOrEmpty(start) && DateTimeOffset.TryParse(start, out var s)) startMs = s.ToUnixTimeMilliseconds();
 if (!string.IsNullOrEmpty(end) && DateTimeOffset.TryParse(end, out var e)) endMs = e.ToUnixTimeMilliseconds();
 var list = await _ts.GetRangeAsync(symbol, startMs, endMs, limit);
 return Ok(list);
 }
 }
}