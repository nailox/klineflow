using System.Collections.Generic;
using System.Threading.Tasks;
using klineflow.Models;

namespace klineflow.Repositories
{
 public interface ICandleRepository
 {
 Task AddRangeAsync(List<Candle> candles);
 Task<List<Candle>> GetRecentAsync(string symbol, int take);
 }
}