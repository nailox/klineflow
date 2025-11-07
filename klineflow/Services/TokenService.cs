using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace klineflow.Services
{
 public class TokenService
 {
 private readonly IConfiguration _config;
 private readonly SymmetricSecurityKey _signingKey;

 public TokenService(IConfiguration config)
 {
 _config = config;
 var keyString = _config.GetSection("Jwt").GetValue<string>("Key") ?? throw new ArgumentException("Jwt:Key is not configured");
 byte[] keyBytes;
 try
 {
 keyBytes = Convert.FromBase64String(keyString);
 }
 catch
 {
 keyBytes = Encoding.UTF8.GetBytes(keyString);
 }

 if (keyBytes.Length <32) throw new ArgumentException("Jwt:Key must be at least32 bytes for HS256");

 _signingKey = new SymmetricSecurityKey(keyBytes);
 }

 public string CreateToken(string username, string role = "Admin", int validHours =2)
 {
 var cfg = _config.GetSection("Jwt");
 var issuer = cfg.GetValue<string>("Issuer");
 var audience = cfg.GetValue<string>("Audience");

 var claims = new[]
 {
 new Claim(ClaimTypes.Name, username),
 new Claim(ClaimTypes.Role, role)
 };

 var token = new JwtSecurityToken(
 issuer: issuer,
 audience: audience,
 claims: claims,
 expires: DateTime.UtcNow.AddHours(validHours),
 signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
 );

 return new JwtSecurityTokenHandler().WriteToken(token);
 }
 }
}
