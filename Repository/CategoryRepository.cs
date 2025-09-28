using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Model;
using be_dotnet_ecommerce1.Repository.IReopsitory;
using Microsoft.EntityFrameworkCore;

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
    public List<CategoryAdmin> getCategoryAdmin()
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
                ORDER BY cat.id;
            ";

      var list = _connect.Set<CategoryAdmin>().FromSqlRaw(sql).ToList();
      return list;
    }
  }
}