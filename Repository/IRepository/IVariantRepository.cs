using be_dotnet_ecommerce1.Dtos;

namespace be_dotnet_ecommerce1.Repository.IRepository
{
    public interface IVariantRepository
    {
        public Task<List<VariantDTO>> getValueVariant(int id);
    }
}