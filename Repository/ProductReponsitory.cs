// File: Repository/ProductReponsitory.cs
using System.Text.Json;
using be_dotnet_ecommerce1.Data;
using dotnet.Dtos;
using dotnet.Dtos.admin;
using dotnet.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace dotnet.Repository
{
  public class ProductReponsitory : IProductReponsitory
  {
    private readonly ConnectData _connect;
    public ProductReponsitory(ConnectData connect)
    {
      _connect = connect;
    }

    public List<ProductDTO> getProductAdmin(int page, int size)
    {
      if (page < 1) page = 1;
      if (size <= 0) size = 10;
      var offset = (page - 1) * size;

      var sql = @"
        WITH product_agg AS (
          SELECT 
              p.id AS product_id,
              p.nameproduct AS product_name,
              p.description AS product_description,
              p.brand,
              p.category AS category_id,
              c.namecategory AS category_name,
              p.imageurls,
              p.createdate AS product_created,
              p.updatedate AS product_updated,
              COALESCE(SUM(v.stock), 0) AS total_stock,
              COALESCE(
                json_agg(
                  json_build_object(
                    'id', v.id,
                    'value', v.valuevariant,      -- JSON object -> JsonElement
                    'stock', v.stock,
                    'inputPrice', v.inputprice,
                    'price', v.price,
                    'createDate', v.createdate,
                    'updateDate', v.updatedate
                  )
                ) FILTER (WHERE v.id IS NOT NULL), '[]'
              )::text AS variants               -- ép ra TEXT để deserialize thủ công
          FROM product p
          LEFT JOIN category c ON p.category = c.id
          LEFT JOIN variant v ON v.product_id = p.id AND v.isdeleted = FALSE
          WHERE p.isdeleted = FALSE
          GROUP BY p.id, c.id, p.nameproduct, p.description, p.brand, p.imageurls, p.createdate, p.updatedate
        )
        SELECT *
        FROM product_agg
        ORDER BY product_id
        LIMIT {0} OFFSET {1};
      ";

      // ❌ KHÔNG dùng Set<ProductDTO>()
      var rows = _connect.Set<ProductAdminRow>()
                         .FromSqlRaw(sql, size, offset)
                         .AsNoTracking()
                         .ToList();

      var list = rows.Select(r =>
      {
        List<ProductDTO.ValueVariant> variants;
        try
        {
          variants = JsonSerializer.Deserialize<List<ProductDTO.ValueVariant>>(r.variants)
                     ?? new List<ProductDTO.ValueVariant>();
        }
        catch
        {
          variants = new List<ProductDTO.ValueVariant>();
        }

        return new ProductDTO
        {
          product_id = r.product_id,
          product_name = r.product_name,
          product_description = r.product_description,
          brand = r.brand,
          category_id = r.category_id,
          category_name = r.category_name,
          imageurls = r.imageurls,
          product_created = r.product_created,
          product_updated = r.product_updated,
          total_stock = r.total_stock,
          variants = variants
        };
      }).ToList();

      return list;
    }
  }
}
