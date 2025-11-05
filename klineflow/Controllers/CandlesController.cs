using Microsoft.AspNetCore.Mvc;
using klineflow.Services;
using klineflow.DTOs;
using System.Threading.Tasks;
using klineflow.Models;

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
 public async Task<IActionResult> FetchAndStore([FromBody] ForecastRequestDto req)
 {
 var candles = await _binance.FetchCandlesAsync(req.Symbol, req.Interval, req.Limit);
 await _ts.StoreCandlesAsync(candles);
 return Ok(new { stored = candles.Count });
 }

 [HttpPost("forecast")]
 public async Task<IActionResult> Forecast([FromBody] ForecastRequestDto req)
 {
 var candles = await _ts.GetRecentAsync(req.Symbol, req.Limit);
 var result = _forecast.Forecast(candles);
 return Ok(result);
 }
 }
}