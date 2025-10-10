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
        private VariantRepository variantRepository;
        public ProductRepository(ConnectData connect)
        {
            _connect = connect;
            variantRepository = new VariantRepository(_connect);
        }

        public async Task<List<Product>> excuteQuery(string sql)
        {
            var result = await _connect.products
                            .FromSqlRaw(sql)
                            .ToListAsync();
            return result;
        }

        // public async Task<List<ProductFilterDTO>> getProductByFilter(FilterDTO dTO)
        // {
        //     var variants = await variantRepository.GetVariantByFilter(dTO);
        //     var productids = variants.Select(v => v.productid).Distinct().ToList();
        //     // var query = from p in _connect.products
        //     //             join v in _connect.variants on p.id equals v.productid // nối bảng variant
        //     //             join c in _connect.categories on p.category equals c.id // nối bảng category

        //     //             join dp in _connect.discountProducts on v.id equals dp.variantid into dpj//nối bảng discoutn product
        //     //             from dp in dpj.DefaultIfEmpty()

        //     //             join d in _connect.discounts on dp.discountid equals d.id into dj //nối bảng discount
        //     //             from d in dj.DefaultIfEmpty()

        //     //             join o in _connect.orders on v.id equals o.variantid into oj // nối bảng order
        //     //             from o in oj.DefaultIfEmpty()

        //     //             join r in _connect.reviews on o.id equals r.orderid into rj // nối bảng review
        //     //             from r in rj.DefaultIfEmpty()
        //     //             where producids.Contains(p.id)
        //     //             select new { p, v, c, dp, d, o, r };
        //     var products = await _connect.products
        //     .Where(p => productids.Contains(p.id))
        //     .Select(p => new ProductFilterDTO
        //     {
        //         id = p.id,
        //         name = p.nameproduct,
        //         description = p.description,
        //         brand = p.brand,
        //         categoryId = p.categoryId,
        //         categoryName = p.Category.namecategory,
        //         imgUrls = p.imageurls,
        //         variant = _connect.variants.Where(v => v.productid == p.id)
        //         .Select(v => new VariantDTO
        //         {
        //             id = v.id,
        //             valuevariant = v.valuevariant,
        //             stock = v.stock,
        //             inputprice = v.inputprice,
        //             price = v.price,
        //             createdate = v.createdate,
        //             updatedate = v.updatedate
        //         }).ToArray(),
        //         discount = (from dp in _connect.discountProducts
        //                     join d in _connect.discounts on dp.discountid equals d.id
        //                     join v in _connect.variants on dp.variantid equals v.id
        //                     select d
        //         ).ToArray(),
        //         rating = (from r in _connect.reviews
        //                   join o in _connect.orders on r.orderid equals o.id
        //                   join v in _connect.variants on o.variantid equals v.id
        //                   where (v.productid == p.id)
        //                   select (int?)r.rating).Sum() ?? 0,

        //         order = (from o in _connect.orders
        //                  join v in _connect.variants on o.variantid equals v.id
        //                  where v.productid == p.id
        //                  select o).Count()
        //     }).ToListAsync();
        //     return products;
        // }

        public async Task<List<ProductFilterDTO>> getProductByFilter(FilterDTO dTO)
        {
            try
            {

                var variants = await variantRepository.GetVariantByFilter(dTO) ?? new List<Variant>();// dữ liệu ngoài database
                var producids = variants.Select(v => v.productid).Distinct().ToList();
                var productRaw = await _connect.products //client side
                    .Include(p => p.Category)
                    .Where(p => producids.Contains(p.id))
                    .ToListAsync();// nếu không toListAsync thì EF core không dịch được. varaints khai báo ngoài nên không ánh xạ được
                var products = productRaw
                    .Select(p => new ProductFilterDTO
                    {
                        id = p.id,
                        name = p.nameproduct,
                        description = p.description,
                        brand = p.brand,
                        categoryId = p.categoryId,
                        categoryName = p.Category != null ? p.Category.namecategory : null,
                        imgUrls = p.imageurls,
                        variant = variants
                            .Where(v => v.productid == p.id)
                            .Select(v => new VariantDTO
                            {
                                id = v.id,
                                valuevariant = v.valuevariant,
                                stock = v.stock,
                                inputprice = v.inputprice,
                                price = v.price,
                                createdate = v.createdate,
                                updatedate = v.updatedate
                            }).ToArray(),
                        discount = (from dp in _connect.discountProducts
                                    join d in _connect.discounts on dp.discountid equals d.id
                                    join v in _connect.variants on dp.variantid equals v.id
                                    where v.productid == p.id
                                    select d).Distinct().ToArray(),
                        rating = (from r in _connect.reviews
                                  join o in _connect.orders on r.orderid equals o.id
                                  join v in _connect.variants on o.variantid equals v.id
                                  where (v.productid == p.id)
                                  select (int?)r.rating).Sum() ?? 0,
                        order = (from o in _connect.orders
                                 join v in _connect.variants on o.variantid equals v.id
                                 where v.productid == p.id
                                 select o).Count()
                    }).ToList();
                return products;
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                throw;
            }

        }



        public int getQuantityByIdCategory(int id)
        {
            var quantity = _connect.products
                                   .Count(p => p.categoryId == id);
            return quantity;
        }

    }
}