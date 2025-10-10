using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Dtos;
using dotnet.Model;

namespace be_dotnet_ecommerce1.Repository
{
    public interface IProductRepository
    {
        public int getQuantityByIdCategory(int id);
        public Task<List<ProductFilterDTO>> getProductByFilter(FilterDTO dTO);
        public Task<List<Product>> excuteQuery(string sql);
    }
}