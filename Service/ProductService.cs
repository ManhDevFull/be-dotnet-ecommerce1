using dotnet.Dtos;
using dotnet.Repository.IRepository;
using dotnet.Service.IService;

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
  }
}