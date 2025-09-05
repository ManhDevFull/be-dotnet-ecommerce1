using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Lỗi: Connection string được khai báo trực tiếp
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// Đăng ký NpgsqlDataSource vào DI container
// Sửa: Sử dụng connection string từ biến môi trường
builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("DefaultConnection")!); // Sửa: Lấy connection string từ cấu hình

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowNext", policy =>
  {
    policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
  });
});
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRouting();

// Enable CORS
app.UseCors("AllowNext");
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

app.UseAuthorization();

app.MapControllers();
app.Run();