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
        public IActionResult getValueVariant(int id)
        {
            var list = _service.getValueVariant(id);
            return Ok(list);
        }
    }
}