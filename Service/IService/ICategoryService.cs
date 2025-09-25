using be_dotnet_ecommerce1.Dtos;
using be_dotnet_ecommerce1.Model;

namespace be_dotnet_ecommerce1.Service.IService
{
  public interface ICategoryService
  {
    public List<CategoryDTO> getCategoryParentById(int? id);
    public List<CategoryAdmin> getCategoryAdmin();
  }
}