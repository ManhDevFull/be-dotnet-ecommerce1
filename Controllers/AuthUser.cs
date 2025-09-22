using Microsoft.AspNetCore.Mvc;
using Npgsql;
using dotnet.Dtos;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace dotnet.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly NpgsqlDataSource _dataSource;
    private readonly IConfiguration _config;
    private readonly HttpClient _http;
    public AuthController(NpgsqlDataSource dataSource, IConfiguration config, IHttpClientFactory httpClientFactory)
    {
      _dataSource = dataSource;
      _config = config;
      _http = httpClientFactory.CreateClient();
    }
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
      try
      {
        await using var conn = await _dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT _id, password, first_name, last_name, rule, avatar_img FROM account WHERE email = @e",
            conn
        );
        cmd.Parameters.AddWithValue("e", dto.Email);
        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
          return Unauthorized(new { message = "Email not found" });

        var hashedPass = reader.GetString(1);
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, hashedPass))
          return Unauthorized(new { message = "Invalid password" });

        var userId = reader.GetInt32(0);
        var userEmail = dto.Email;
        var userName = $"{reader.GetString(2)} {reader.GetString(3)}";
        var rule = reader.GetInt32(4).ToString();
        string? avatar = null;
        if (!reader.IsDBNull(5))
        {
          avatar = reader.GetString(5);
        }
        // tạo access token + refresh token
        var accessToken = GenerateJwtToken(userId.ToString(), userEmail, rule);
        var refreshToken = GenerateRefreshToken();

        reader.Close();

        await using var updateCmd = new NpgsqlCommand(
          "UPDATE account SET refresh_token = @rt, refresh_token_expires = @exp WHERE _id = @id",
          conn
        );
        updateCmd.Parameters.AddWithValue("rt", refreshToken);
        updateCmd.Parameters.AddWithValue("exp", DateTime.UtcNow.AddDays(7));
        updateCmd.Parameters.AddWithValue("id", userId);
        await updateCmd.ExecuteNonQueryAsync();

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
          data = new
          {
            accessToken,
            user = new { id = userId, name = userName, email = userEmail, avatarUrl = avatar, rule }
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
          await using var conn = await _dataSource.OpenConnectionAsync();
          await using var cmd = new NpgsqlCommand(
            "UPDATE account SET refresh_token = NULL, refresh_token_expires = NULL WHERE refresh_token = @rt",
            conn
          );
          cmd.Parameters.AddWithValue("rt", cookieRt);
          await cmd.ExecuteNonQueryAsync();
        }

        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
          HttpOnly = true,
          Secure = true,
          SameSite = SameSiteMode.Strict,
          Path = "/"
        });

        return Ok(new { status = 200, message = "Logged out" });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { error = ex.Message });
      }
    }

    [HttpPost("social-auth")]
    public async Task<IActionResult> ExchangeFirebaseToken([FromBody] TokenRequest dto)
    {
      if (string.IsNullOrEmpty(dto.IdToken))
        return BadRequest(new { message = "Missing Firebase IdToken" });

      try
      {
        // 1) Verify token với Firebase Admin
        var decoded = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance
            .VerifyIdTokenAsync(dto.IdToken);

        var uid = decoded.Uid;
        var email = decoded.Claims.ContainsKey("email") ? decoded.Claims["email"]?.ToString() : null;
        var name = decoded.Claims.ContainsKey("name") ? decoded.Claims["name"]?.ToString() : null;
        var avatarUrl = decoded.Claims.ContainsKey("picture") ? decoded.Claims["picture"]?.ToString() : null;

        await using var conn = await _dataSource.OpenConnectionAsync();

        int userId;
        string rule;

        await using (var checkCmd = new NpgsqlCommand("SELECT _id, rule FROM account WHERE email = @e", conn))
        {
          checkCmd.Parameters.AddWithValue("e", (object?)email ?? DBNull.Value);
          await using var reader = await checkCmd.ExecuteReaderAsync();
          if (await reader.ReadAsync())
          {
            userId = reader.GetInt32(0);
            rule = reader.GetInt32(1).ToString();
            reader.Close();
          }
          else
          {
            reader.Close();
            var parts = (name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var fn = parts.Length > 0 ? parts[0] : "";
            var ln = parts.Length > 1 ? parts[^1] : "";

            await using var insertCmd = new NpgsqlCommand(
                "INSERT INTO account (email, first_name, last_name, avatar_img, rule) VALUES (@e, @fn, @ln, @avt, @rule) RETURNING _id",
                conn
            );
            insertCmd.Parameters.AddWithValue("e", (object?)email ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("fn", (object?)fn ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("ln", (object?)ln ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("rule", 3);
            insertCmd.Parameters.AddWithValue("avt", (object?)avatarUrl ?? DBNull.Value);
            var scalar = await insertCmd.ExecuteScalarAsync();
            if (scalar == null) return StatusCode(500, new { message = "Insert user failed" });
            userId = Convert.ToInt32(scalar);
            rule = "3";
          }
        }

        var accessToken = GenerateJwtToken(userId.ToString(), email ?? $"{uid}@firebase.com", rule);
        var refreshToken = GenerateRefreshToken();

        await using (var updateCmd = new NpgsqlCommand(
    "UPDATE account SET refresh_token = @rt, refresh_token_expires = @exp WHERE _id = @id", conn))
        {
          updateCmd.Parameters.AddWithValue("rt", refreshToken);
          updateCmd.Parameters.AddWithValue("exp", DateTime.UtcNow.AddDays(7));
          updateCmd.Parameters.AddWithValue("id", userId);
          await updateCmd.ExecuteNonQueryAsync();
        }

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
          data = new
          {
            accessToken,
            user = new { id = userId, name, avatarUrl, email, rule }
          }
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

        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
          "SELECT _id, email, rule, refresh_token_expires FROM account WHERE refresh_token = @rt",
          conn
        );
        cmd.Parameters.AddWithValue("rt", refreshTokenToCheck);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
          return Unauthorized(new { message = "Invalid refresh token" });

        var userId = reader.GetInt32(0).ToString();
        var email = reader.GetString(1);
        var rule = reader.GetInt32(2).ToString();
        var expires = reader.GetDateTime(3);

        if (expires < DateTime.UtcNow)
          return Unauthorized(new { message = "Refresh token expired" });

        var newRefreshToken = GenerateRefreshToken();
        reader.Close();

        await using var updateCmd = new NpgsqlCommand(
          "UPDATE account SET refresh_token = @newRt, refresh_token_expires = @exp WHERE _id = @id",
          conn
        );
        updateCmd.Parameters.AddWithValue("newRt", newRefreshToken);
        updateCmd.Parameters.AddWithValue("exp", DateTime.UtcNow.AddDays(7));
        updateCmd.Parameters.AddWithValue("id", int.Parse(userId));
        await updateCmd.ExecuteNonQueryAsync();

        var newAccessToken = GenerateJwtToken(userId, email, rule);

        var cookieOptions = new CookieOptions
        {
          HttpOnly = true,
          Secure = true,
          SameSite = SameSiteMode.Strict,
          Expires = DateTimeOffset.UtcNow.AddDays(7),
          Path = "/"
        };
        Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

        return Ok(new
        {
          status = 200,
          data = new { accessToken = newAccessToken }
        });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { error = ex.Message });
      }
    }

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
          expires: DateTime.UtcNow.AddMinutes(1),
          signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
