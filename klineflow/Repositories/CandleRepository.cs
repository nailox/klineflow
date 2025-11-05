using System.Collections.Generic;
using System.Threading.Tasks;
using klineflow.Models;
using klineflow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace klineflow.Repositories
{
 public class CandleRepository : ICandleRepository
 {
 private readonly AppDbContext _db;
 public CandleRepository(AppDbContext db)
 {
 _db = db;
 }

 public async Task AddRangeAsync(List<Candle> candles)
 {
 await _db.Candles.AddRangeAsync(candles);
 await _db.SaveChangesAsync();
 }

 public async Task<List<Candle>> GetRecentAsync(string symbol, int take)
 {
 return await _db.Candles.Where(c => c.Symbol == symbol).OrderByDescending(c => c.OpenTime).Take(take).ToListAsync();
 }
 }
}