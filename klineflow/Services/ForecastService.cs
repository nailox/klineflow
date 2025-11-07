using klineflow.Models;
using System.Text;
using System.Text.Json;

namespace klineflow.Services
{
 public class ForecastService
 {
 private readonly HttpClient _http;
 private readonly ILogger<ForecastService> _logger;
 private readonly string _geminiApiKey;

 public ForecastService(HttpClient http, ILogger<ForecastService> logger, IConfiguration config)
 {
 _http = http;
 _logger = logger;
 _geminiApiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey");
 }

 public async Task<string> AnalyzeWithGeminiAsync(List<Candle> candles)
 {
 // Ensure we have up to100 candles; caller should provide100 but guard here
 var last100 = (candles ?? new List<Candle>()).OrderByDescending(c => c.OpenTime).Take(100).OrderBy(c => c.OpenTime).ToList();

 // Fixed system and user prompt
 var systemPrompt = "You are an expert quantitative analyst and technical trader. You analyze OHLCV candlestick data, extract trends, detect patterns, calculate key indicators, identify turning points, and forecast short-term movements.";
 var userInstruction =
 "Here are100 days of candlestick data in JSON format. Each item contains: timestamp, open, high, low, close, volume. Analyze the trend, volatility, patterns, momentum, and important technical levels. Then give a forecast for the next5–7 days.\n\nProvide your output in this format:\n\n1. Overall market trend\n2. Momentum analysis (RSI, MACD interpretation)\n3. Support & resistance levels\n4. Pattern detection (triangles, flags, double top/bottom, etc)\n5. Volatility analysis\n6. Risk factors\n7. Probability-based forecast for the next week (bullish/bearish %, expected range)\n8. Summary in simple words.";

 // Build the JSON payload of candles in the requested simplified form
 var candleArray = last100.Select(c => new
 {
 timestamp = c.OpenTime,
 open = c.Open,
 high = c.High,
 low = c.Low,
 close = c.Close,
 volume = c.Volume
 }).ToList();
 var candlesJson = JsonSerializer.Serialize(candleArray);

 // Combine prompts and data into one text input
 var combined = new StringBuilder();
 combined.AppendLine(systemPrompt);
 combined.AppendLine();
 combined.AppendLine(userInstruction);
 combined.AppendLine();
 combined.AppendLine(candlesJson);

 // Build request body per requested shape
 var bodyObj = new
 {
 contents = new[]
 {
 new
 {
 parts = new[]
 {
 new { text = combined.ToString() }
 }
 }
 }
 };

 var json = JsonSerializer.Serialize(bodyObj);

 // Log payload being sent (Info length, Debug full)
 try
 {
 _logger.LogInformation("Sending payload to Gemini (length={Length})", json?.Length ??0);
 _logger.LogDebug("Gemini payload: {Payload}", json);
 }
 catch { /* ignore logging errors */ }

 // Prepare request
 var request = new HttpRequestMessage(HttpMethod.Post, "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent")
 {
 Content = new StringContent(json, Encoding.UTF8, "application/json")
 };
 // add x-goog-api-key header from configuration
 request.Headers.Add("x-goog-api-key", _geminiApiKey);

 var res = await _http.SendAsync(request);
 res.EnsureSuccessStatusCode();
 var resp = await res.Content.ReadAsStringAsync();
 return resp;
 }
 }
}