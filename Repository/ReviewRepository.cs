using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Repository.IRepository;
using dotnet.Model;
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

        public async Task<Dictionary<int, int>> getSumQuantityReviewByIdProduct(List<int> ids) // đếm số luowngj review theo mã sản phẩm
        {
          if (ids == null)
                return new Dictionary<int, int>();
            var result = await (
                from v in _connect.variants
                join o in _connect.orders on v.id equals o.variantid
                join r in _connect.reviews on o.id equals r.orderid
                where ids.Contains(v.productid)
                group r by v.productid into g
                select new
                {
                    productid = g.Key,
                    reviewcount = g.Count()
                }
            ).ToListAsync();
            return result.ToDictionary(x => x.productid, x => x.reviewcount);
        }


        public async Task<Dictionary<int, int>> getSumRatingByIdsProduct(List<int> ids) // tổng số rating theo mã sản phâm
        {
            if (ids == null)
                return new Dictionary<int, int>();
            var result = await (
                from v in _connect.variants
                join o in _connect.orders on v.id equals o.variantid
                join r in _connect.reviews on o.id equals r.orderid
                where ids.Contains(v.productid)
                group r by v.productid into g
                select new
                {
                    productid = g.Key,
                    reviewsum = g.Sum(x => x.rating)
                }
            ).ToListAsync();
            return result.ToDictionary(x => x.productid, x => x.reviewsum);
        }

    }
}