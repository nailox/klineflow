using System.Collections.Generic;
using System.Threading.Tasks;
using klineflow.Clients;
using klineflow.Models;
using System.Linq;

namespace klineflow.Services
{
 public class BinanceService
 {
 private readonly BinanceApiClient _client;
 public BinanceService(BinanceApiClient client)
 {
 _client = client;
 }

 public async Task<List<Candle>> FetchCandlesAsync(string symbol, string interval, int limit =500)
 {
 var klines = await _client.GetKlinesAsync(symbol, interval, limit);
 var candles = klines.Select(k => new Candle
 {
 OpenTime = System.Convert.ToInt64(k[0]),
 Open = System.Convert.ToDecimal(k[1]),
 High = System.Convert.ToDecimal(k[2]),
 Low = System.Convert.ToDecimal(k[3]),
 Close = System.Convert.ToDecimal(k[4]),
 Volume = System.Convert.ToDecimal(k[5]),
 CloseTime = System.Convert.ToInt64(k[6]),
 QuoteAssetVolume = System.Convert.ToDecimal(k[7]),
 NumberOfTrades = System.Convert.ToInt32(k[8]),
 TakerBuyBaseAssetVolume = System.Convert.ToDecimal(k[9]),
 TakerBuyQuoteAssetVolume = System.Convert.ToDecimal(k[10]),
 Symbol = symbol
 }).ToList();
 return candles;
 }
 }
}