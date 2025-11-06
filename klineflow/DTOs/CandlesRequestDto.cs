namespace klineflow.DTOs
{
 public class CandlesRequestDto
 {
 public string Symbol { get; set; } = string.Empty;
 public string Interval { get; set; } = "1h";
 public int Limit { get; set; } =100;
 }
}
