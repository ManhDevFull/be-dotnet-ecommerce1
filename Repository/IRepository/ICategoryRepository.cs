using be_dotnet_ecommerce1.Dtos;
using be_dotnet_ecommerce1.Model;
using dotnet.Model;

namespace be_dotnet_ecommerce1.Repository.IReopsitory
{
    public interface ICategoryRepository
    {
        public List<Category> getParentById(int? id);
        public List<CategoryAdminDTO> getCategoryAdmin();
        public List<BrandOptionDTO> getBrandByCate(int? categoryId);
    }
}
