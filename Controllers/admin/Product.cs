using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using be.Service.IService;
using dotnet.Dtos;
using dotnet.Dtos.admin;
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
    public async Task<IActionResult> GetProductAdmin([FromQuery] AdminProductFilter query)
    {
      int page = query.Page ?? 1;
      int size = query.Size ?? 30;
      string? name = query.name;
      int? cate = query.cate;
      string? brand = query.brand;
      bool? stock = query.stock;
      string sort = (query.sort ?? "newest").ToLowerInvariant();
      sort = sort switch
      {
        "priceasc" => "price_asc",
        "pricedesc" => "price_desc",
        "newest" => "newest",
        _ => "newest"
      };

      var result = await _service.getProductAdmin(page, size, name, cate, brand, stock, sort);
      return Ok(new
      {
        status = 200,
        data = result,
        message = "Success"
      });
    }
    // [HttpDelete]
    // [Authorize(Roles = "0")]
    // public async Task<IActionResult> DeleteProductAdmin([FromBody] RequestDel req)
    // {
    //   return Ok(new
    //   {
    //     status = 200,
    //     message = "Success"
    //   });
    // }
  }
}