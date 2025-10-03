using System.Text.Json;
using be_dotnet_ecommerce1.Service.IService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace be_dotnet_ecommerce1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VariantController : ControllerBase
    {
        private readonly IVariantService _service;
        public VariantController(IVariantService service)
        {
            _service = service;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> getValueVariant(int id)
        {
            var list = await _service.getValueVariant(id);
            return Ok(list);
        }
        [HttpPost]
        public IActionResult resFilter(FilterDTO dTO)
        {
            Console.WriteLine((JsonSerializer.Serialize(dTO)));
            return Ok(dTO);
        }
    }
}