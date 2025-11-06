using klineflow.DTOs;
using klineflow.Services;
using Microsoft.AspNetCore.Mvc;

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
 await _ts.StoreCandlesAsync(candles);
 return Ok(new { stored = candles.Count });
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
 public async Task<IActionResult> DeleteAll()
 {
 await _ts.DeleteAllAsync();
 return Ok(new { deleted = true });
 }
 }
}