using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("DefaultConnection")!); // Sửa: Lấy connection string từ cấu hình

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowNext", policy =>
  {
    policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
             .AllowCredentials();
  });
});
builder.Services.AddOpenApi();
var jwtConfig = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
      ValidIssuer = jwtConfig["Issuer"],
      ValidAudience = jwtConfig["Audience"],
      IssuerSigningKey = new SymmetricSecurityKey(key),
      NameClaimType = ClaimTypes.NameIdentifier,
      RoleClaimType = ClaimTypes.Role
    };
     options.Events = new JwtBearerEvents
    {
      OnAuthenticationFailed = ctx =>
      {
        Console.WriteLine("JWT Authentication Failed: " + ctx.Exception?.Message);
        return Task.CompletedTask;
      }
    };
  });
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowNext");
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
  if (context.Request.Method == HttpMethods.Options)
  {
    context.Response.StatusCode = StatusCodes.Status204NoContent;
    return;
  }
  await next();
});

app.MapGet("/",
    async () =>
    {
      try
      {
        // Sửa: Sử dụng connection string từ cấu hình
        await using var conn = new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")!); // Sửa: Lấy connection string từ cấu hình
        await conn.OpenAsync();

        return Results.Json(new { message = "Connection Success" });
      }
      catch (Exception ex)
      {
        return Results.Json(new { message = "Connection Fail", error = ex.Message });
      }
    });


app.MapControllers();
app.Run();