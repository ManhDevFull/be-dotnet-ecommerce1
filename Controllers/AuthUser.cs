using Microsoft.AspNetCore.Mvc;
using Npgsql;
using dotnet.Dtos;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace dotnet.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly NpgsqlDataSource _dataSource;
    private readonly IConfiguration _config;
    public AuthController(NpgsqlDataSource dataSource, IConfiguration config)
    {
      _dataSource = dataSource;
      _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
      try
      {
        await using var conn = await _dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT _id, password, first_name, last_name, rule FROM account WHERE email = @e",
            conn
        );
        cmd.Parameters.AddWithValue("e", dto.Email);
        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
          return Unauthorized(new { message = "Email not found" });

        var hashedPass = reader.GetString(1);
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, hashedPass))
          return Unauthorized(new { message = "Invalid password" });

        var userId = reader.GetInt32(0).ToString();
        var userEmail = dto.Email;
        var userName = $"{reader.GetString(2)} {reader.GetString(3)}";
        var rule = reader.GetInt32(4).ToString();

        // tạo access token + refresh token
        var accessToken = GenerateJwtToken(userId, userEmail, rule);
        var refreshToken = GenerateRefreshToken();

        reader.Close();

        // lưu refresh token vào DB
        await using var updateCmd = new NpgsqlCommand(
          "UPDATE account SET refresh_token = @rt, refresh_token_expires = @exp WHERE _id = @id",
          conn
        );
        updateCmd.Parameters.AddWithValue("rt", refreshToken);
        updateCmd.Parameters.AddWithValue("exp", DateTime.UtcNow.AddDays(7));
        updateCmd.Parameters.AddWithValue("id", int.Parse(userId));
        await updateCmd.ExecuteNonQueryAsync();

        var cookieOptions = new CookieOptions
        {
          HttpOnly = true,
          Secure = true,
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
            user = new { id = userId, name = userName, email = userEmail, rule }
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
