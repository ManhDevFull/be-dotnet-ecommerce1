
using dotnet.Dtos;
using dotnet.Repository.IRepository;
using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Dtos;
using be_dotnet_ecommerce1.Service.IService;
using be.Service.IService;

namespace dotnet.Service
{
  public class ProductService : IProductService
  {
    private readonly IProductReponsitory _repo;
    public ProductService(IProductReponsitory repo)
    {
      _repo = repo;
    }
    public List<ProductDTO> getProductAdmin(int page, int size)
    {
      var list = _repo.getProductAdmin(page, size);
      return list;
    }

    public async Task<List<ProductFilterDTO>> getProductByFilter(FilterDTO dTO)
    {
      var result = await _repo.getProductByFilter(dTO);
      return result;
    }


    public int getQuantityByIdCategory(int id)
    {
      var quantity = _repo.getQuantityByIdCategory(id);
      return quantity;
    }
  }
}

