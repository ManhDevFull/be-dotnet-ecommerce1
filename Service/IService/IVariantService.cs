using be_dotnet_ecommerce1.Dtos;

namespace be_dotnet_ecommerce1.Service.IService
{
    public interface IVariantService
    {
        public Task<List<VariantDTO>> getValueVariant(int id);
    }
}