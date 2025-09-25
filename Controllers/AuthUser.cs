using Microsoft.AspNetCore.Mvc;
using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Model;
using dotnet.Dtos;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;

namespace dotnet.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase
  {
    // OLD: private readonly NpgsqlDataSource _dataSource;
    private readonly ConnectData _db; // NEW
    private readonly IConfiguration _config;
    private readonly HttpClient _http;

    // OLD:
    // public AuthController(NpgsqlDataSource dataSource, IConfiguration config, IHttpClientFactory httpClientFactory)
    // {
    //     _dataSource = dataSource;
    //     _config = config;
    //     _http = httpClientFactory.CreateClient();
    // }

    // NEW:
    public AuthController(ConnectData db, IConfiguration config, IHttpClientFactory httpClientFactory)
    {
      _db = db;
      _config = config;
      _http = httpClientFactory.CreateClient();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
      try
      {
        var user = await _db.Set<Account>().FirstOrDefaultAsync(u => u.email == dto.Email);
        if (user == null)
          return Unauthorized(new { message = "Email not found" });

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.password))
          return Unauthorized(new { message = "Invalid password" });

        var accessToken = GenerateJwtToken(user.id.ToString(), user.email, user.role.ToString());
        var refreshToken = GenerateRefreshToken();

        user.refreshtoken = refreshToken;
        user.refreshtokenexpires = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync();


        var accessCookieOptions = new CookieOptions
        {
          HttpOnly = true,
          Secure = false,
          SameSite = SameSiteMode.Lax,
          Expires = DateTimeOffset.UtcNow.AddMinutes(15),
          Path = "/"
        };
        Response.Cookies.Append("accessToken", accessToken, accessCookieOptions);

        // cookie for refresh token (long lived)
        var refreshCookieOptions = new CookieOptions
        {
          HttpOnly = true,
          Secure = false,
          SameSite = SameSiteMode.Lax,
          Expires = DateTimeOffset.UtcNow.AddDays(7),
          Path = "/"
        };
        Response.Cookies.Append("refreshToken", refreshToken, refreshCookieOptions);

        return Ok(new
        {
          status = 200,
          data = new
          {
            accessToken,
            user = new { id = user.id, name = $"{user.firstname} {user.lastname}", email = user.email, avatarUrl = user.avatarimg, rule = user.role }
          }
        });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { error = ex.Message });
      }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
      try
      {
        var cookieRt = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(cookieRt))
        {
          var user = await _db.Set<Account>().FirstOrDefaultAsync(u => u.refreshtoken == cookieRt);
          if (user != null)
          {
            user.refreshtoken = null;
            user.refreshtokenexpires = null;

            _db.Entry(user).Property(u => u.refreshtoken).IsModified = true;
            _db.Entry(user).Property(u => u.refreshtokenexpires).IsModified = true;

            var result = await _db.SaveChangesAsync();
            Console.WriteLine($"Rows affected: {result}");

          }
        }

        Response.Cookies.Delete("refreshToken");
        Response.Cookies.Delete("accessToken");

        return Ok(new { status = 200, message = "Logged out successfully" });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { error = ex.Message });
      }
    }

    [HttpPost("social-auth")]
    [AllowAnonymous]
    public async Task<IActionResult> ExchangeFirebaseToken([FromBody] TokenRequest dto)
    {
      if (string.IsNullOrEmpty(dto.IdToken))
        return BadRequest(new { message = "Missing Firebase IdToken" });
      try
      {
        // Verify với Firebase
        var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(dto.IdToken);

        var uid = decoded.Uid;
        var email = decoded.Claims.ContainsKey("email") ? decoded.Claims["email"]?.ToString() : null;
        var name = decoded.Claims.ContainsKey("name") ? decoded.Claims["name"]?.ToString() : null;
        var avatarUrl = decoded.Claims.ContainsKey("picture") ? decoded.Claims["picture"]?.ToString() : null;

        // OLD: query tay
        // await using var conn = await _dataSource.OpenConnectionAsync();
        // int userId;
        // string rule;

        // NEW: EF Core
        var user = await _db.Set<Account>().FirstOrDefaultAsync(u => u.email == email);
        if (user == null)
        {
          var parts = (name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
          var fn = parts.Length > 0 ? parts[0] : "";
          var ln = parts.Length > 1 ? parts[^1] : "";

          user = new Account
          {
            email = email ?? $"{uid}@firebase.com",
            firstname = fn,
            lastname = ln,
            avatarimg = avatarUrl,
            role = 3
          };
          _db.Add(user);
          await _db.SaveChangesAsync();
        }

        var accessToken = GenerateJwtToken(user.id.ToString(), user.email, user.role.ToString());
        var refreshToken = GenerateRefreshToken();

        user.refreshtoken = refreshToken;
        user.refreshtokenexpires = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync();

        var cookieOptions = new CookieOptions
        {
          HttpOnly = true,
          Secure = false,
          SameSite = SameSiteMode.Strict,
          Expires = DateTimeOffset.UtcNow.AddDays(7),
          Path = "/"
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

        return Ok(new
        {
          status = 200,
          data = new { accessToken, user = new { id = user.id, name, avatarUrl, email, rule = user.role } }
        });
      }
      catch (Exception ex)
      {
        return Unauthorized(new { message = "Invalid Firebase IdToken", detail = ex.Message });
      }
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest dto)
    {
      try
      {
        var providedRt = dto?.RefreshToken;
        var cookieRt = Request.Cookies["refreshToken"];
        var refreshTokenToCheck = !string.IsNullOrEmpty(providedRt) ? providedRt : cookieRt;

        if (string.IsNullOrEmpty(refreshTokenToCheck))
          return Unauthorized(new { message = "No refresh token provided" });

        // OLD: Npgsql
        // await using var conn = await _dataSource.OpenConnectionAsync();
        // await using var cmd = new NpgsqlCommand("SELECT _id, email, rule, refresh_token_expires FROM account WHERE refresh_token = @rt", conn);
        // cmd.Parameters.AddWithValue("rt", refreshTokenToCheck);

        // NEW: EF Core
        var user = await _db.Set<Account>().FirstOrDefaultAsync(u => u.refreshtoken == refreshTokenToCheck);
        if (user == null || user.refreshtokenexpires < DateTime.UtcNow)
          return Unauthorized(new { message = "Invalid or expired refresh token" });

        var newRefreshToken = GenerateRefreshToken();
        user.refreshtoken = newRefreshToken;
        user.refreshtokenexpires = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync();

        var newAccessToken = GenerateJwtToken(user.id.ToString(), user.email, user.role.ToString());

        var cookieOptions = new CookieOptions
        {
          HttpOnly = true,
          Secure = true,
          SameSite = SameSiteMode.Strict,
          Expires = DateTimeOffset.UtcNow.AddDays(7),
          Path = "/"
        };
        Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

        return Ok(new { status = 200, data = new { accessToken = newAccessToken } });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { error = ex.Message });
      }
    }

    // ===== Helper functions =====
    private string GenerateRefreshToken()
    {
      return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    private string GenerateJwtToken(string userId, string email, string rule)
    {
      var jwtKey = _config["Jwt:Key"];
      var jwtIssuer = _config["Jwt:Issuer"];
      var jwtAudience = _config["Jwt:Audience"];

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var claims = new[]
      {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, rule),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

      var token = new JwtSecurityToken(
          issuer: jwtIssuer,
          audience: jwtAudience,
          claims: claims,
          expires: DateTime.UtcNow.AddMinutes(60),
          signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
