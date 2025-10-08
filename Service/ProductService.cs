using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Dtos;
using be_dotnet_ecommerce1.Repository;

namespace be_dotnet_ecommerce1.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        public ProductService(IProductRepository repo)
        {
            _repo = repo;
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