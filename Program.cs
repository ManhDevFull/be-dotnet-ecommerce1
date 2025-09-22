using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Security.Claims;
using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using be_dotnet_ecommerce1.Data;
using Microsoft.EntityFrameworkCore;
using be_dotnet_ecommerce1.Service.IService;
using be_dotnet_ecommerce1.Repository.IReopsitory;
using be_dotnet_ecommerce1.Repository;

var builder = WebApplication.CreateBuilder(args);

// DB
// builder.Services.AddNpgsqlDataSource(
//     builder.Configuration.GetConnectionString("DefaultConnection")!
// );
builder.Services.AddDbContext<ConnectData>(options =>
{
  options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
// đăng ký tự động inject thông qua constructor
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
// CORS
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowNext", policy =>
  {
    policy.WithOrigins("http://localhost:3000", "https://vertex-ecom.vercel.app")
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
// Chay local thi bat doan nay len
if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile("vertex.json")
    });
}
// Push len git thi bat doan nay len
// if (FirebaseApp.DefaultInstance == null)
// {
//     var json = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS_JSON");
//     FirebaseApp.Create(new AppOptions()
//     {
//         Credential = GoogleCredential.FromJson(json)
//     });
// }


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
app.MapControllers();
app.Run();
