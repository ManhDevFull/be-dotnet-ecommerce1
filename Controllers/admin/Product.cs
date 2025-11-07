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
using Microsoft.AspNetCore.Http;
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
    [HttpPost]
    [Authorize(Roles = "0")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductAdminCreateRequest request)
    {
      try
      {
        var created = await _service.CreateProductAsync(request);
        return StatusCode(StatusCodes.Status201Created, new
        {
          status = 201,
          data = created,
          message = "Created"
        });
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { status = 400, message = ex.Message });
      }
    }

    [HttpPut("{productId:int}")]
    [Authorize(Roles = "0")]
    public async Task<IActionResult> UpdateProduct(int productId, [FromBody] ProductAdminUpdateRequest request)
    {
      try
      {
        var updated = await _service.UpdateProductAsync(productId, request);
        if (updated == null)
        {
          return NotFound(new { status = 404, message = "Product not found" });
        }
        return Ok(new { status = 200, data = updated, message = "Updated" });
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { status = 400, message = ex.Message });
      }
    }

    [HttpDelete("{productId:int}")]
    [Authorize(Roles = "0")]
    public async Task<IActionResult> DeleteProduct(int productId)
    {
      var deleted = await _service.DeleteProductAsync(productId);
      if (!deleted)
      {
        return NotFound(new { status = 404, message = "Product not found" });
      }

      return Ok(new { status = 200, message = "Deleted" });
    }

    [HttpPost("{productId:int}/variant")]
    [Authorize(Roles = "0")]
    public async Task<IActionResult> CreateVariant(int productId, [FromBody] VariantAdminCreateRequest request)
    {
      try
      {
        var updated = await _service.CreateVariantAsync(productId, request);
        if (updated == null)
        {
          return NotFound(new { status = 404, message = "Product not found" });
        }
        return Ok(new { status = 200, data = updated, message = "Variant created" });
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { status = 400, message = ex.Message });
      }
    }

    [HttpPut("{productId:int}/variant/{variantId:int}")]
    [Authorize(Roles = "0")]
    public async Task<IActionResult> UpdateVariant(int productId, int variantId, [FromBody] VariantAdminUpdateRequest request)
    {
      try
      {
        var updated = await _service.UpdateVariantAsync(productId, variantId, request);
        if (updated == null)
        {
          return NotFound(new { status = 404, message = "Variant not found" });
        }
        return Ok(new { status = 200, data = updated, message = "Variant updated" });
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { status = 400, message = ex.Message });
      }
    }

    [HttpDelete("{productId:int}/variant/{variantId:int}")]
    [Authorize(Roles = "0")]
    public async Task<IActionResult> DeleteVariant(int productId, int variantId)
    {
      var updated = await _service.DeleteVariantAsync(productId, variantId);
      if (updated == null)
      {
        return NotFound(new { status = 404, message = "Variant not found" });
      }
      return Ok(new { status = 200, data = updated, message = "Variant deleted" });
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
