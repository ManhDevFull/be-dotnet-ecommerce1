using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddNpgsqlDataSource(
    builder.Configuration.GetConnectionString("DefaultConnection")!
);

builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowNext", policy =>
  {
    policy.WithOrigins("http://localhost:3000", "https://fe-ecommerce1.vercel.app")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
  });
});

builder.Services.AddOpenApi();

// JWT config từ env
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey))
  throw new Exception("JWT Key is missing. Please set env JWT__KEY");

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
      ValidIssuer = jwtIssuer,
      ValidAudience = jwtAudience,
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

// handle preflight
app.Use(async (context, next) =>
{
  if (context.Request.Method == HttpMethods.Options)
  {
    context.Response.StatusCode = StatusCodes.Status204NoContent;
    return;
  }
  await next();
});

// test db connection
app.MapGet("/",
    async () =>
    {
      try
      {
        await using var conn = new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")!);
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
