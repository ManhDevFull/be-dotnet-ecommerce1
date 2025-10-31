using Microsoft.AspNetCore.Mvc;
using be_dotnet_ecommerce1.Data;
using dotnet.Model;
using be_dotnet_ecommerce1.Model;
using dotnet.Dtos;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using dotnet.Service.Email;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

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
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<AuthController> _logger;

    // OLD:
    // public AuthController(NpgsqlDataSource dataSource, IConfiguration config, IHttpClientFactory httpClientFactory)
    // {
    //     _dataSource = dataSource;
    //     _config = config;
    //     _http = httpClientFactory.CreateClient();
    // }

    // NEW:
    public AuthController(
      ConnectData db,
      IConfiguration config,
      IHttpClientFactory httpClientFactory,
      IEmailService emailService,
      IOptions<EmailSettings> emailOptions,
      ILogger<AuthController> logger)
    {
      _db = db;
      _config = config;
      _http = httpClientFactory.CreateClient();
      _emailService = emailService;
      _emailSettings = emailOptions.Value;
      _logger = logger;
    }

    [HttpPost("request-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestEmailVerification([FromBody] RegisterRequest dto, CancellationToken cancellationToken)
    {
      try
      {
        var email = dto.Email?.Trim().ToLowerInvariant();
        var password = dto.Password?.Trim();
        var fullName = dto.FullName?.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullName))
        {
          return BadRequest(new { message = "Email, password and full name are required." });
        }

        if (!IsValidEmail(email))
        {
          return BadRequest(new { message = "Invalid email address." });
        }

        if (password.Length < 6)
        {
          return BadRequest(new { message = "Password must be at least 6 characters long." });
        }

        var existingAccount = await _db.accounts.AnyAsync(u => u.email == email, cancellationToken);
        if (existingAccount)
        {
          return Conflict(new { message = "Email is already registered. Please sign in instead." });
        }

        var now = DateTime.UtcNow;
        var verification = await _db.emailVerifications.FirstOrDefaultAsync(v => v.email == email, cancellationToken);

        if (verification == null)
        {
          verification = new EmailVerification
          {
            email = email,
            createdat = now
          };
          await _db.emailVerifications.AddAsync(verification, cancellationToken);
        }
        else
        {
          var cooldownSeconds = Math.Max(0, _emailSettings.ResendCooldownSeconds);
          if (verification.lastsentat.HasValue && cooldownSeconds > 0)
          {
            var secondsSinceLast = (int)Math.Floor((now - verification.lastsentat.Value).TotalSeconds);
            if (secondsSinceLast < cooldownSeconds)
            {
              var waitSeconds = Math.Max(0, cooldownSeconds - secondsSinceLast);
              return StatusCode(StatusCodes.Status429TooManyRequests, new
              {
                message = $"Please wait {waitSeconds} second(s) before requesting another code.",
                retryAfter = waitSeconds
              });
            }
          }
        }

        var code = GenerateVerificationCode();
        var hashedCode = BCrypt.Net.BCrypt.HashPassword(code);
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var (firstName, lastName) = SplitFullName(fullName);

        verification.codehash = hashedCode;
        verification.passwordhash = hashedPassword;
        verification.firstname = string.IsNullOrWhiteSpace(firstName) ? null : firstName;
        verification.lastname = string.IsNullOrWhiteSpace(lastName) ? null : lastName;
        verification.expiresat = now.AddMinutes(Math.Max(1, _emailSettings.CodeExpiryMinutes));
        verification.attemptcount = 0;
        verification.updatedat = now;
        verification.lastsentat = now;

        await _db.SaveChangesAsync(cancellationToken);

        var subject = "Verify your email address";
        var body = BuildVerificationEmailBody(fullName!, code, _emailSettings.CodeExpiryMinutes);
        await _emailService.SendAsync(email, subject, body, cancellationToken);

        return Ok(new { status = 200, message = "Verification code sent to your email." });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to request email verification for {Email}", dto?.Email);
        return StatusCode(500, new { message = "Failed to send verification code." });
      }
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest dto, CancellationToken cancellationToken)
    {
      try
      {
        var email = dto.Email?.Trim().ToLowerInvariant();
        var code = dto.Code?.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
        {
          return BadRequest(new { message = "Email and verification code are required." });
        }

        var verification = await _db.emailVerifications.FirstOrDefaultAsync(v => v.email == email, cancellationToken);
        if (verification == null)
        {
          return BadRequest(new { message = "Verification session not found. Please request a new code." });
        }

        if (verification.expiresat < DateTime.UtcNow)
        {
          return BadRequest(new { message = "Verification code expired. Please request a new code." });
        }

        var maxAttempts = Math.Max(1, _emailSettings.MaxAttempts);
        if (verification.attemptcount >= maxAttempts)
        {
          return StatusCode(StatusCodes.Status423Locked, new { message = "Too many invalid attempts. Please request a new verification code." });
        }

        if (!BCrypt.Net.BCrypt.Verify(code, verification.codehash))
        {
          verification.attemptcount += 1;
          await _db.SaveChangesAsync(cancellationToken);
          var attemptsLeft = Math.Max(0, maxAttempts - verification.attemptcount);
          return BadRequest(new
          {
            message = attemptsLeft > 0
              ? $"Invalid verification code. {attemptsLeft} attempt(s) remaining."
              : "Invalid verification code."
          });
        }

        var existingAccount = await _db.accounts.FirstOrDefaultAsync(u => u.email == email, cancellationToken);
        if (existingAccount != null)
        {
          _db.emailVerifications.Remove(verification);
          await _db.SaveChangesAsync(cancellationToken);
          return Conflict(new { message = "Email already registered. Please sign in instead." });
        }

        var account = new Account
        {
          email = email,
          firstname = verification.firstname,
          lastname = verification.lastname,
          password = verification.passwordhash,
          role = 3,
          createdate = DateTime.UtcNow,
          updatedate = DateTime.UtcNow
        };

        var refreshToken = GenerateRefreshToken();
        account.refreshtoken = refreshToken;
        account.refreshtokenexpires = DateTime.UtcNow.AddDays(7);

        _db.accounts.Add(account);
        _db.emailVerifications.Remove(verification);
        await _db.SaveChangesAsync(cancellationToken);

        var accessToken = GenerateJwtToken(account.id.ToString(), account.email ?? string.Empty, account.role.ToString());

        var accessCookieOptions = new CookieOptions
        {
          HttpOnly = true,
          Secure = false,
          SameSite = SameSiteMode.Lax,
          Expires = DateTimeOffset.UtcNow.AddMinutes(15),
          Path = "/"
        };
        Response.Cookies.Append("accessToken", accessToken, accessCookieOptions);

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
            user = new
            {
              id = account.id,
              name = $"{account.firstname} {account.lastname}".Trim(),
              email = account.email,
              avatarUrl = account.avatarimg,
              rule = account.role
            }
          }
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to verify email for {Email}", dto?.Email);
        return StatusCode(500, new { message = "Failed to verify email." });
      }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
      try
      {
        var user = await _db.accounts.FirstOrDefaultAsync(u => u.email == dto.Email);
        if (user == null)
          return Unauthorized(new { message = "Email not found" });

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.password))
          return Unauthorized(new { message = "Invalid password" });

        var accessToken = GenerateJwtToken(user.id.ToString(), dto.Email ?? string.Empty, user.role.ToString());
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
            user = new { id = user.id, name = $"{user.firstname} {user.lastname}", email = dto.Email, avatarUrl = user.avatarimg, rule = user.role }
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
          var user = await _db.accounts.FirstOrDefaultAsync(u => u.refreshtoken == cookieRt);
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
        var user = await _db.accounts.FirstOrDefaultAsync(u => u.email == email);
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

        var accessToken = GenerateJwtToken(user.id.ToString(), user.email ?? string.Empty, user.role.ToString());
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
        var user = await _db.accounts.FirstOrDefaultAsync(u => u.refreshtoken == refreshTokenToCheck);
        if (user == null || user.refreshtokenexpires < DateTime.UtcNow)
          return Unauthorized(new { message = "Invalid or expired refresh token" });

        var newRefreshToken = GenerateRefreshToken();
        user.refreshtoken = newRefreshToken;
        user.refreshtokenexpires = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync();

        var newAccessToken = GenerateJwtToken(user.id.ToString(), user.email ?? string.Empty, user.role.ToString());

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

    private static string GenerateVerificationCode()
    {
      Span<byte> buffer = stackalloc byte[4];
      RandomNumberGenerator.Fill(buffer);
      var value = BitConverter.ToUInt32(buffer) % 1000000;
      return value.ToString("D6");
    }

    private static (string firstName, string lastName) SplitFullName(string fullName)
    {
      var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length == 0)
        return (string.Empty, string.Empty);
      if (parts.Length == 1)
        return (parts[0], string.Empty);
      var firstName = parts[0];
      var lastName = string.Join(' ', parts.Skip(1));
      return (firstName, lastName);
    }

    private bool IsValidEmail(string email)
    {
      try
      {
        var address = new MailAddress(email);
        return address.Address.Equals(email, StringComparison.OrdinalIgnoreCase);
      }
      catch
      {
        return false;
      }
    }

    private string BuildVerificationEmailBody(string fullName, string code, int expiryMinutes)
    {
      var safeName = string.IsNullOrWhiteSpace(fullName) ? "there" : WebUtility.HtmlEncode(fullName);
      var safeCode = WebUtility.HtmlEncode(code);
      var expiryText = expiryMinutes <= 1 ? "1 minute" : $"{expiryMinutes} minutes";

      return $@"
<p>Hi {safeName},</p>
<p>Your verification code is <strong style=""font-size:20px;"">{safeCode}</strong>.</p>
<p>This code will expire in {expiryText}. If you did not request this, you can safely ignore this email.</p>
<p>Thanks,<br/>Vertex E-commerce Team</p>";
    }
  }
}
