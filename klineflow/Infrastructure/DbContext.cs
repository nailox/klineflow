using Microsoft.EntityFrameworkCore;
using klineflow.Models;

namespace klineflow.Infrastructure
{
 public class AppDbContext : DbContext
 {
 public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
 {
 }

 public DbSet<Candle> Candles { get; set; }
 }
}