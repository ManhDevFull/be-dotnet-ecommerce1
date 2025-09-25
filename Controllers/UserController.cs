using Microsoft.AspNetCore.Authorization; // ✅ thêm để dùng [Authorize]
using Microsoft.AspNetCore.Mvc;
using be_dotnet_ecommerce1.Data;
using System.Security.Claims;

namespace dotnet.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class UserController : ControllerBase
  {
       private readonly ConnectData _db;

        public UserController(ConnectData db)
        {
            _db = db;
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
