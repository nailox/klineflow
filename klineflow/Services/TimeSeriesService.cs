using System.Collections.Generic;
using System.Threading.Tasks;
using klineflow.Models;
using klineflow.Repositories;
using System.Linq;

namespace klineflow.Services
{
 public class TimeSeriesService
 {
 private readonly ICandleRepository _repo;
 public TimeSeriesService(ICandleRepository repo)
 {
 _repo = repo;
 }

 public async Task StoreCandlesAsync(List<Candle> candles)
 {
 await _repo.AddRangeAsync(candles);
 }

 public async Task<List<Candle>> GetRecentAsync(string symbol, int take =100)
 {
 return await _repo.GetRecentAsync(symbol, take);
 }
 }
}