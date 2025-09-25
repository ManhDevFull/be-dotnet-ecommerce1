using be_dotnet_ecommerce1.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace dotnet.Controllers.admin
{
  [Route("admin/[controller]")]
  [ApiController]
  public class CategoryController : ControllerBase
  {
    private readonly ICategoryService _service;
    public CategoryController(ICategoryService service)
    {
      _service = service;
    }
    [HttpGet]
    [Authorize(Roles = "0")]
    public IActionResult CategoryParentAdmin()
    {
      var list = _service.getCategoryAdmin();
      return Ok(new {
        status = 200,
        data= list,
        message = "Success"
      });
    }
  }
}