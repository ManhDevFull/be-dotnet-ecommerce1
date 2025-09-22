using be_dotnet_ecommerce1.Dtos;

namespace be_dotnet_ecommerce1.Service.IService
{
    public interface ICategoryService
    {
        public List<CategoryDTO> getCategoryParentById(int? id);
    }
}