using System.Net.WebSockets;
using System.Runtime.Intrinsics.Arm;
using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Dtos;
using be_dotnet_ecommerce1.Model;
using be_dotnet_ecommerce1.Repository.IRepository;
using dotnet.Model;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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


        public async Task<List<ProductFilterDTO>> getProductBySql(string sql)
        {
            var rawData = await _connect.Set<V_ProductFilter>()
            .FromSqlRaw(sql)
            .ToListAsync();
            var rs = rawData.Select(r => new ProductFilterDTO
            {
                id = r.id,
                name = r.name,
                description = r.description,
                brand = r.brand,
                categoryId = r.categoryId,
                categoryName = r.categoryName,
                imgUrls = r.imgUrls,
                variant = string.IsNullOrEmpty(r.variant) 
            ? null 
            : JsonSerializer.Deserialize<List<VariantDTO>>(r.variant),

                rating = r.rating,
                order = r.order
            }).ToList();
            return rs;
        }

        public int getQuantityByIdCategory(int id)
        {
            var quantity = _connect.products
                                   .Count(p => p.categoryId == id);
            return quantity;
        }

    }
}