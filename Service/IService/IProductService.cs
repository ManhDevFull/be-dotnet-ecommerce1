using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Dtos;
using dotnet.Dtos;
using dotnet.Dtos.admin;

namespace be.Service.IService
{
  public interface IProductService
  {

    public int getQuantityByIdCategory(int id);
    public Task<List<ProductFilterDTO>> getProductByFilter(FilterDTO dTO);
    public Task<PagedResult<ProductAdminDTO>> getProductAdmin(
    int page,
    int size,
    string? name,
    int? cate,
    string? brand,
    bool? stock,
    string sort = "newest");
  }

}