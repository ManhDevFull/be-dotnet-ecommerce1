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
                // OLD: query thủ công bằng Npgsql
                // await using var conn = await _dataSource.OpenConnectionAsync();
                // await using var cmd = new NpgsqlCommand("SELECT _id, password, first_name, last_name, rule, avatar_img FROM account WHERE email = @e", conn);
                // cmd.Parameters.AddWithValue("e", dto.Email);
                // await using var reader = await cmd.ExecuteReaderAsync();
                // if (!await reader.ReadAsync()) return Unauthorized(new { message = "Email not found" });
                // var hashedPass = reader.GetString(1);
                // if (!BCrypt.Net.BCrypt.Verify(dto.Password, hashedPass)) return Unauthorized(new { message = "Invalid password" });

                // NEW: EF Core
                var user = await _db.Set<Account>().FirstOrDefaultAsync(u => u.email == dto.Email);
                if (user == null)
                    return Unauthorized(new { message = "Email not found" });

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.password))
                    return Unauthorized(new { message = "Invalid password" });

                var accessToken = GenerateJwtToken(user._id.ToString(), user.email, user.rule.ToString());
                var refreshToken = GenerateRefreshToken();

                user.refresh_token = refreshToken;
                user.refresh_token_expires = DateTime.UtcNow.AddDays(7);
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
                    data = new
                    {
                        accessToken,
                        user = new { id = user._id, name = $"{user.first_name} {user.last_name}", email = user.email, avatarUrl = user.avatar_img, rule = user.rule }
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

                // OLD: Npgsql update
                // if (!string.IsNullOrEmpty(cookieRt))
                // {
                //     await using var conn = await _dataSource.OpenConnectionAsync();
                //     await using var cmd = new NpgsqlCommand("UPDATE account SET refresh_token = NULL, refresh_token_expires = NULL WHERE refresh_token = @rt", conn);
                //     cmd.Parameters.AddWithValue("rt", cookieRt);
                //     await cmd.ExecuteNonQueryAsync();
                // }

                // NEW: EF Core
                if (!string.IsNullOrEmpty(cookieRt))
                {
                    var user = await _db.Set<Account>().FirstOrDefaultAsync(u => u.refresh_token == cookieRt);
                    if (user != null)
                    {
                        user.refresh_token = null;
                        user.refresh_token_expires = null;
                        await _db.SaveChangesAsync();
                    }
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
                        first_name = fn,
                        last_name = ln,
                        avatar_img = avatarUrl,
                        rule = 3
                    };
                    _db.Add(user);
                    await _db.SaveChangesAsync();
                }

                var accessToken = GenerateJwtToken(user._id.ToString(), user.email, user.rule.ToString());
                var refreshToken = GenerateRefreshToken();

                user.refresh_token = refreshToken;
                user.refresh_token_expires = DateTime.UtcNow.AddDays(7);
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
                    data = new { accessToken, user = new { id = user._id, name, avatarUrl, email, rule = user.rule } }
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
                var user = await _db.Set<Account>().FirstOrDefaultAsync(u => u.refresh_token == refreshTokenToCheck);
                if (user == null || user.refresh_token_expires < DateTime.UtcNow)
                    return Unauthorized(new { message = "Invalid or expired refresh token" });

                var newRefreshToken = GenerateRefreshToken();
                user.refresh_token = newRefreshToken;
                user.refresh_token_expires = DateTime.UtcNow.AddDays(7);
                await _db.SaveChangesAsync();

                var newAccessToken = GenerateJwtToken(user._id.ToString(), user.email, user.rule.ToString());

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
