using be_dotnet_ecommerce1.Dtos;
using be_dotnet_ecommerce1.Repository.IRepository;
using be_dotnet_ecommerce1.Service.IService;

namespace be_dotnet_ecommerce1.Service
{
    public class VariantService : IVariantService
    {
        private readonly IVariantRepository _repo;
        public VariantService(IVariantRepository repo)
        {
            _repo = repo;
        }
        public List<VariantDTO> getValueVariant(int id)
        {
            return _repo.getValueVariant(id);
        }

    }
}