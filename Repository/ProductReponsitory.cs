using System.Text.Json;
using be_dotnet_ecommerce1.Data;
using dotnet.Dtos;
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
                  'variant_id', v.id,
                  'valuevariant', v.valuevariant,
                  'stock', v.stock,
                  'inputprice', v.inputprice,
                  'price', v.price,
                  'createdate', v.createdate,
                  'updatedate', v.updatedate
                )
              ) FILTER (WHERE v.id IS NOT NULL), '[]'
            )::text AS variants
        FROM product p
        LEFT JOIN category c ON p.category = c.id
        LEFT JOIN variant v ON v.product_id = p.id AND v.isdeleted = FALSE
        WHERE p.isdeleted = FALSE
        GROUP BY p.id, c.id, p.nameproduct, p.description, p.brand, p.imageurls, p.createdate, p.updatedate
      )
      SELECT * FROM product_agg
      ORDER BY product_id
      LIMIT {0} OFFSET {1};
    ";

    var rows = _connect.Set<ProductDTO>()
                       .FromSqlRaw(sql, size, offset)
                       .AsNoTracking()
                       .ToList();

    var result = rows.Select(r => new ProductDTO
    {
        id = r.id,
        name = r.name,
        description = r.description,
        brand = r.brand,
        categoryId = r.categoryId,
        categoryName = r.categoryName,
        imgUrls = r.imgUrls.ToList(),
        createDate = r.createDate,
        updateDate = r.updateDate,
        totalStock = r.totalStock,
        variant = JsonSerializer.Deserialize<List<ProductDTO.ValueVariant>>(r.variant)
    }).ToList();

    return result;
}
  }
}