using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace be_dotnet_ecommerce1.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ConnectData _connect;
        public OrderRepository(ConnectData connect)
        {
            _connect = connect;
        }
        public async Task<int> getQuantityOrderByIdProduct(int id)
        {
            var result = await (from v in _connect.variants
                                join o in _connect.orders on v.id equals o.variantid
                                where v.productid == id
                                select o).CountAsync();
            return result;
        }

    }
}