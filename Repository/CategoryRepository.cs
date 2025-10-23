using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Dtos;
using dotnet.Model;
using be_dotnet_ecommerce1.Repository.IReopsitory;
using Microsoft.EntityFrameworkCore;
using be_dotnet_ecommerce1.Model;

namespace be_dotnet_ecommerce1.Repository
{

  public class CategoryRepository : ICategoryRepository
  {
    private readonly ConnectData _connect;
    public CategoryRepository(ConnectData connect)
    {
      _connect = connect;
    }
    public List<Category> getParentById(int? id)
    {
      return _connect.categories.Where(c => c.idparent == id).ToList();
    }
    public List<CategoryAdminDTO> getCategoryAdmin()
    {
      var sql = @"
                WITH RECURSIVE descendants AS (
                SELECT id AS root_id, id
                FROM category
                UNION ALL
                SELECT d.root_id, c.id
                FROM category c
                JOIN descendants d ON c.parent_id = d.id
              )
              SELECT
                cat.id,
                cat.namecategory,
                cat.parent_id AS idparent,
                COUNT(DISTINCT p.id) AS product
              FROM category cat
              LEFT JOIN descendants d ON d.root_id = cat.id
              LEFT JOIN product p ON p.category = d.id
              GROUP BY cat.id, cat.namecategory, cat.parent_id
              ORDER BY cat.id
            ";

      return _connect.categoryAdmins.FromSqlRaw(sql).AsNoTracking().ToList();
    }

    public List<BrandOptionDTO> getBrandByCate(int? categoryId)
    {
      if (categoryId.HasValue)
      {
        return (from stats in _connect.category_brand_stats.AsNoTracking()
                join brand in _connect.brands.AsNoTracking() on stats.brand_id equals brand.id
                where stats.category_id == categoryId.Value
                orderby brand.name
                select new BrandOptionDTO
                {
                  id = brand.id,
                  name = brand.name
                }).ToList();
      }

      return _connect.brands
        .AsNoTracking()
        .OrderBy(b => b.name)
        .Select(b => new BrandOptionDTO
        {
          id = b.id,
          name = b.name
        })
        .ToList();
    }
  }
}
