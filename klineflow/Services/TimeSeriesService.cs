using System.Collections.Generic;
using System.Threading.Tasks;
using klineflow.Models;
using klineflow.Clients;

namespace klineflow.Services
{
 public class TimeSeriesService
 {
 private readonly SupabaseRestClient _supabase;
 public TimeSeriesService(SupabaseRestClient supabase)
 {
 _supabase = supabase;
 }

 public async Task StoreCandlesAsync(List<Candle> candles)
 {
 await _supabase.InsertCandlesAsync(candles);
 }

 public async Task<List<Candle>> GetRecentAsync(string symbol, int take =100)
 {
 return await _supabase.GetRecentAsync(symbol, take);
 }
 }
}