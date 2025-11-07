using klineflow.Services;
using Microsoft.AspNetCore.Mvc;

namespace klineflow.Controllers
{
 [ApiController]
 [Route("api/[controller]")]
 public class AuthController : ControllerBase
 {
 private readonly TokenService _tokenService;
 private readonly IConfiguration _config;

 public AuthController(TokenService tokenService, IConfiguration config)
 {
 _tokenService = tokenService;
 _config = config;
 }

 public class TokenRequest
 {
 public string Username { get; set; } = string.Empty;
 public string Password { get; set; } = string.Empty;
 }

 [HttpPost("token")]
 public IActionResult GetToken([FromBody] TokenRequest req)
 {
 var cfg = _config.GetSection("Jwt");
 var adminUser = cfg.GetValue<string>("AdminUser");
 var adminPass = cfg.GetValue<string>("AdminPassword");
 if (req.Username != adminUser || req.Password != adminPass) return Unauthorized();
 var token = _tokenService.CreateToken(req.Username);
 return Ok(new { token });
 }
 }
}
