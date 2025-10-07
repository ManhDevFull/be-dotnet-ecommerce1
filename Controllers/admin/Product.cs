using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using dotnet.Dtos;
using dotnet.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dotnet.Controllers.admin
{
  [Route("admin/[controller]")]
  [ApiController]
  public class ProductController : ControllerBase
  {
    private readonly IProductService _service;
    public ProductController(IProductService service)
    {
      _service = service;
    }
    [HttpGet]
    [Authorize(Roles = "0")]
    public IActionResult GetAddress([FromQuery] BaseRequest query)
    {
            int page = query.Page ?? 1;
      int size = query.Size ?? 10;
      // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var list = _service.getProductAdmin(page, size);
      return Ok(new
      {
        status = 200,
        data = new
        {
          listProduct = list,
          page ,
         size
        },
        message = "Success"
      });
    }
  }
}