using klineflow.Clients;
using klineflow.Models;
using System.Globalization;
using System.Text.Json;

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

 static long GetLong(JsonElement e)
 {
 if (e.ValueKind == JsonValueKind.Number && e.TryGetInt64(out var v)) return v;
 if (e.ValueKind == JsonValueKind.String && long.TryParse(e.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var s)) return s;
 throw new System.InvalidCastException($"Cannot convert element {e} to long");
 }
 static int GetInt(JsonElement e)
 {
 if (e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var v)) return v;
 if (e.ValueKind == JsonValueKind.String && int.TryParse(e.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var s)) return s;
 // some Binance fields like number of trades may exceed int range rarely; fall back via long then cast
 var l = GetLong(e);
 return (int)l;
 }
 static decimal GetDecimal(JsonElement e)
 {
 if (e.ValueKind == JsonValueKind.Number && e.TryGetDecimal(out var v)) return v;
 if (e.ValueKind == JsonValueKind.String && decimal.TryParse(e.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var s)) return s;
 throw new System.InvalidCastException($"Cannot convert element {e} to decimal");
 }

 var candles = klines.Select(k => new Candle
 {
 OpenTime = GetLong(k[0]),
 Open = GetDecimal(k[1]),
 High = GetDecimal(k[2]),
 Low = GetDecimal(k[3]),
 Close = GetDecimal(k[4]),
 Volume = GetDecimal(k[5]),
 CloseTime = GetLong(k[6]),
 QuoteAssetVolume = GetDecimal(k[7]),
 NumberOfTrades = GetInt(k[8]),
 TakerBuyBaseAssetVolume = GetDecimal(k[9]),
 TakerBuyQuoteAssetVolume = GetDecimal(k[10]),
 Symbol = symbol
 }).ToList();
 return candles;
 }
 }
}