using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace be_dotnet_ecommerce1.Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ConnectData _connect;
        public ReviewRepository(ConnectData connect)
        {
            _connect = connect;
        }

        public async Task<int> getSumQuantityReviewByIdProduct(int id) // đếm số luowngj review theo mã sản phẩm
        {
            var result = await (
                from v in _connect.variants
                join o in _connect.orders on v.id equals o.variantid
                join r in _connect.reviews on o.id equals r.orderid
                where v.productid == id
                select r
            ).CountAsync();
            return result;
        }


        public async Task<int> getSumRatingByIdProduct(int id) // tổng số rating theo mã sản phâm
        {
            var result = await (
                from v in _connect.variants
                join o in _connect.orders on v.id equals o.variantid
                join r in _connect.reviews on o.id equals r.orderid
                where v.productid == id
                select (int?)r.rating
            ).SumAsync() ?? 0;
            return result;
        }

    }
}