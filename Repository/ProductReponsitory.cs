// File: Repository/ProductReponsitory.cs
using System.Text.Json;
using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Dtos;
using be_dotnet_ecommerce1.Repository;
using be_dotnet_ecommerce1.Repository.IRepository;
using dotnet.Dtos;
using dotnet.Model;
using dotnet.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using dotnet.Dtos.admin;

namespace dotnet.Repository
{
  public class ProductReponsitory : IProductReponsitory
  {
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


    public async Task<PagedResult<ProductAdminDTO>> getProductAdmin(
        int page,
        int size,
        string? name,
        int? cate,
        string? brand,
        bool? stock,
        string sort = "newest")
    {
      page = Math.Max(1, page);
      size = Math.Clamp(size, 1, 100);
      var offset = (page - 1) * size;

      var q = _connect.productAdmins.AsNoTracking().AsQueryable();

      // ---- FILTER ----
      if (!string.IsNullOrWhiteSpace(name))
        q = q.Where(p => EF.Functions.ILike(p.name!, $"%{name}%")); // ILIKE cho Postgres

      if (cate.HasValue)
        q = q.Where(p => p.category_id == cate.Value);

      if (!string.IsNullOrWhiteSpace(brand))
        q = q.Where(p => p.brand == brand);

      // Lọc tồn kho: có/không có biến thể stock > 0
      if (stock.HasValue)
      {
        if (stock.Value)
        {
          q = from p in q
              where _connect.variants.Any(v => v.productid == p.product_id && v.stock > 0)
              select p;
        }
        else
        {
          q = from p in q
              where !_connect.variants.Any(v => v.productid == p.product_id && v.stock > 0)
              select p;
        }
      }

      // ---- SORT ----
      q = sort switch
      {
        "name_asc" => q.OrderBy(p => p.name),
        "name_desc" => q.OrderByDescending(p => p.name),
        "price_asc" => q.OrderBy(p => p.min_price ?? int.MaxValue),
        "price_desc" => q.OrderByDescending(p => p.min_price ?? int.MinValue),
        "oldest" => q.OrderBy(p => p.createdate ?? DateTime.MinValue),
        "updated" => q.OrderByDescending(p => p.updatedate ?? p.createdate ?? DateTime.MinValue),
        _ => q.OrderByDescending(p => p.createdate ?? DateTime.MinValue) // newest
      };

      // ---- PAGING ----
      var total = await q.CountAsync();
      var rows = await q.Skip(offset).Take(size).ToListAsync();

      return new PagedResult<ProductAdminDTO>
      {
        Items = rows,
        Total = total,
        Page = page,
        Size = size
      };
    }

  }
}
