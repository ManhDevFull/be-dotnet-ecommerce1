using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Dtos;
using dotnet.Model;

namespace be_dotnet_ecommerce1.Repository
{
    public interface IProductRepository
    {
        public int getQuantityByIdCategory(int id);
        public Task<List<Product>> getProductBySql(string sql);
        public Task<int> countProductBySql(string sql);
    }
}