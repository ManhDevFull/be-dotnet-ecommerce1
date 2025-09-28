using be_dotnet_ecommerce1.Dtos;
using be_dotnet_ecommerce1.Model;
using be_dotnet_ecommerce1.Repository.IReopsitory;

namespace be_dotnet_ecommerce1.Service.IService
{
  public class CategoryService : ICategoryService
  {
    private readonly ICategoryRepository _repo;
    public CategoryService(ICategoryRepository repo)
    {
      _repo = repo;
    }
    public List<CategoryDTO> getCategoryParentById(int? id)
    {
      var list = _repo.getParentById(id).Select(c => new CategoryDTO
      {
        _id = c.id,
        name_category = c.namecategory
      }).ToList();
      return list;
    }
    public List<CategoryAdmin> getCategoryAdmin()
    {
      var list = _repo.getCategoryAdmin();
      return list;
    }

  }
}