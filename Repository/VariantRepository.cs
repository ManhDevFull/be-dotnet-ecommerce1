using System.Collections.Generic;
using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Dtos;
using dotnet.Model;
using Microsoft.EntityFrameworkCore;

namespace be_dotnet_ecommerce1.Repository.IRepository
{
    public class VariantRepository : IVariantRepository
    {
        private readonly ConnectData _connect;
        public VariantRepository(ConnectData connect)
        {
            _connect = connect;
        }
        public async Task<List<VariantFilterDTO>> GetValueVariant()
        {
            //     var data = await _connect.Database
            // .SqlQueryRaw<VariantFilterDTO>(@"
            //     SELECT 
            //     key, 
            //     array_agg(DISTINCT value ORDER BY value) AS values
            // FROM (
            //     -- Lấy các thuộc tính từ valuevariant (Không thay đổi)
            //     SELECT 
            //         kv.key::text AS key, 
            //         kv.value::text AS value
            //     FROM category 
            //     JOIN product ON category.id = product.category
            //     JOIN variant ON product.id = variant.product_id
            //     CROSS JOIN LATERAL jsonb_each_text(variant.valuevariant) AS kv(key, value)
            //     --WHERE category.id = 1
            //     --AND variant.isdeleted = false
            //     --AND product.isdeleted = false

            //     UNION ALL

            //     -- Thêm giá như một thuộc tính (Không thay đổi)
            //     SELECT 
            //         'price' AS key,
            //         v.price::text AS value
            //     FROM category c
            //     JOIN product p ON c.id = p.category
            //     JOIN variant v ON p.id = v.product_id
            //     --WHERE c.id = 1
            //     AND v.isdeleted = false
            //     AND p.isdeleted = false

            //     UNION ALL

            //     --- Thêm thương hiệu (ĐÃ CẬP NHẬT)
            //     SELECT
            //         'brand' as key,
            //         b.name::text as value  -- Lấy 'name' từ bảng 'brand'
            //     FROM product p
            //     JOIN brand b ON p.brand_id = b.id -- Join 'product' với 'brand' qua khóa ngoại

            //     UNION ALL

            //     -- thêm danh mục (Không thay đổi)
            //     SELECT
            //         'category' as key,
            //         c.namecategory::text as value
            //     FROM category c
            // ) AS combined
            // GROUP BY key
            // ORDER BY key;
            // ")
            // .ToListAsync();

            //     return data;

            // chuyển sang dùng view
            var data = await _connect.Database.SqlQueryRaw<VariantFilterDTO>(@"SELECT * from v_variant_filters").ToListAsync();
            return data;
        }
        public async Task<List<Variant>> GetVariantByFilter(FilterDTO dTO) // done
        {
            var sql = "select * from variant";
            var conditions = new List<string>();
            var conditionsProduct = new Dictionary<string, List<string>>();
            var conditionsVariant = new Dictionary<string, List<string>>();

            if (dTO.Filter != null)
            {
                foreach (var item in dTO.Filter)
                {
                    var key = item.Key;
                    var value = item.Value.ToList();
                    if (key == "brand" || key == "namecategory")
                        conditionsProduct[key] = value;
                    else
                        conditionsVariant[key] = value;
                }
            }

            if (conditions.Count > 0)
            {
                sql += " where " + string.Join(" AND ", conditions);
                Console.Write(sql);
            }
            var result = await _connect.variants.FromSqlRaw(sql).ToListAsync();
            return result;
        }

        public async Task<Variant[]> GetVariantByIdProduct(int id)
        {
            var result = await _connect.variants
                .Include(p => p.Product)
                .Where(p => p.Product != null && p.Product.id == id)
                .ToArrayAsync();
            return result;
        }

        public async Task<Variant[]> getVariantByIdProducts(List<int> ids) // lấy danh sách variant by list product ids
        {
            if (ids == null)
                return new Variant[0];
            var rs = await _connect.variants.Where(v => ids.Contains(v.productid)).Distinct().ToArrayAsync();
            return rs;
        }

    }
}