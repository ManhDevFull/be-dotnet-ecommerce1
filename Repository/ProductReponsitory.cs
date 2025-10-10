// File: Repository/ProductReponsitory.cs
using System.Text.Json;
using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Dtos;
using be_dotnet_ecommerce1.Repository;
using be_dotnet_ecommerce1.Repository.IRepository;
using dotnet.Dtos;
using dotnet.Dtos.admin;
using dotnet.Model;
using dotnet.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace dotnet.Repository
{
  public class ProductReponsitory : IProductReponsitory  {
    private readonly ConnectData _connect;
    private VariantRepository variantRepository;
    public ProductReponsitory(ConnectData connect)
    {
      _connect = connect;
      variantRepository = new VariantRepository(_connect);
    }

    public async Task<List<ProductFilterDTO>> getProductByFilter(FilterDTO dTO)
    {
      try
      {

        var variants = await variantRepository.GetVariantByFilter(dTO) ?? new List<Variant>();
        var producids = variants.Select(v => v.productid).Distinct().ToList();

        var products = await _connect.products
            .Include(p => p.category)
            .Where(p => producids.Contains(p.id))
            .Select(p => new ProductFilterDTO
            {
              id = p.id,
              name = p.nameproduct,
              description = p.description,
              brand = p.brand,
              categoryId = p.categoryId,
              categoryName = p.category != null ? p.category.namecategory : null,
              imgUrls = p.imageurls,
              variant = _connect.variants
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
                          select d).ToArray(),
              rating = (from r in _connect.reviews
                        join o in _connect.orders on r.orderid equals o.id
                        join v in _connect.variants on o.variantid equals v.id
                        where (v.productid == p.id)
                        select (int?)r.rating).Sum() ?? 0,
              order = (from o in _connect.orders
                       join v in _connect.variants on o.variantid equals v.id
                       where v.productid == p.id
                       select o).Count()
            }).ToListAsync();
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
      var quantity = _connect.products.Count(p => p.categoryId == id);
      return quantity;
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
