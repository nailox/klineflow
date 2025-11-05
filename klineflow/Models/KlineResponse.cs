using System.Collections.Generic;

namespace klineflow.Models
{
 public class KlineResponse
 {
 public List<List<object>> Klines { get; set; } = new();
 }
}