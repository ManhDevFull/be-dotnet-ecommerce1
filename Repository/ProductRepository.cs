using System.Net.WebSockets;
using System.Runtime.Intrinsics.Arm;
using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Dtos;
using be_dotnet_ecommerce1.Model;
using be_dotnet_ecommerce1.Repository.IRepository;
using dotnet.Model;
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

        public async Task<int> countProductBySql(string sql)
        {
            var count = await _connect.Database.SqlQueryRaw<int>(sql).SingleAsync();
            return count;
        }


        public async Task<List<Product>> getProductBySql(string sql)
        {
            var result = await _connect.products
                            .FromSqlRaw(sql)
                            .ToListAsync();
            return result;
        }

        public int getQuantityByIdCategory(int id)
        {
            var quantity = _connect.products
                                   .Count(p => p.categoryId == id);
            return quantity;
        }

    }
}