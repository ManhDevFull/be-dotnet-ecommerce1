using Npgsql;
using Microsoft.AspNetCore.Mvc;

namespace dotnet.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class UserController : ControllerBase
  {
    private readonly NpgsqlDataSource _dataSource;
    public UserController(NpgsqlDataSource dataSource) 
    {
      _dataSource = dataSource;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      try
      {
        var users = new List<object>();

        await using var conn = await _dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand("SELECT _id, nameUser FROM TESTUSER WHERE _id='2'", conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
          users.Add(new
          {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1)
          });
        }
        return Ok(new
        {
          data = users
        });
      }
      catch (Exception ex)
      {
        return BadRequest(new {error= ex});
      }
    }
  }
}