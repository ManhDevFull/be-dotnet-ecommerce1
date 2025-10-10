using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Dtos;
using dotnet.Dtos;

namespace be.Service.IService
{
  public interface IProductService
  {

    public int getQuantityByIdCategory(int id);
    public Task<List<ProductFilterDTO>> getProductByFilter(FilterDTO dTO);
    public List<ProductDTO> getProductAdmin(int page, int size);
  }

}