using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Model;
using Microsoft.EntityFrameworkCore;

namespace be_dotnet_ecommerce1.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ConnectData _connect;
        public ProductRepository(ConnectData connect)
        {
            _connect = connect;
        }
        public int getQuantityByIdCategory(int id)
        {
            var quantity = _connect.products
                                   .Count(p => p.category == id);
            return quantity;
        }

    }
}