using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Repository.IRepository;
using dotnet.Model;
using Microsoft.EntityFrameworkCore;

namespace be_dotnet_ecommerce1.Repository
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly ConnectData _connect;
        public DiscountRepository(ConnectData connect)
        {
            _connect = connect;
        }
        public async Task<Discount> getDiscountByIdProduct(int id)
        {
            var result = await (from p in _connect.products
                                where p.id == id
                                join v in _connect.variants on p.id equals v.productid // nối bảng variant

                                join dp in _connect.discountProducts on v.id equals dp.variantid into dpj//nối bảng discoutn product
                                from dp in dpj.DefaultIfEmpty()

                                join d in _connect.discounts on dp.discountid equals d.id into dj //nối bảng discount
                                from d in dj.DefaultIfEmpty()
                               )
                               return 
        }

    }
}