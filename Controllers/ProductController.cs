using be_dotnet_ecommerce1.Service;
using Microsoft.AspNetCore.Mvc;

namespace be_dotnet_ecommerce1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private IProductService _service;
        public ProductController(IProductService service)
        {
            _service = service;
        }
        [HttpGet("{id}")]
        public IActionResult getQuantityByIdCategory(int id)
        {
            var quantity = _service.getQuantityByIdCategory(id);
            return Ok(quantity);
        }
        [HttpPost("filter")]
        public IActionResult FilterProducts([FromBody] FilterDTO dTO)
        {
            var result = _service.getProductByFilter(dTO);
            return Ok(result);
        }
    }
}