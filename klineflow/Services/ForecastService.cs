using System.Threading.Tasks;
using klineflow.Models;
using System.Linq;
using System.Collections.Generic;

namespace klineflow.Services
{
 public class ForecastService
 {
 // Simple naive forecast: next close = average of last N closes
 public ForecastResult Forecast(List<Candle> candles)
 {
 var result = new ForecastResult();
 if (candles == null || !candles.Any()) return result;
 result.Symbol = candles.First().Symbol;
 result.ForecastedClose = candles.Average(c => c.Close);
 return result;
 }
 }
}