using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Dtos;
using dotnet.Model;

namespace be_dotnet_ecommerce1.Service
{
    public interface IProductService
    {
        public int getQuantityByIdCategory(int id);
        public Task<List<ProductFilterDTO>> getProductByFilter(FilterDTO dTO);
    }
}