using Microsoft.AspNetCore.Authorization; // ✅ thêm để dùng [Authorize]
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using dotnet.Dtos;
using System.Security.Claims;

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
   
    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetAll()
    {
      try
      {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        return Ok(new
        {
          message = "User authenticated succes",
          userId,
          email,
          role
        });
      }
      catch (Exception ex)
      {
        return BadRequest(new { error = ex });
      }
    }
  }
}
