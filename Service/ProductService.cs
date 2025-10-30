using System.ComponentModel;
using System.Net.WebSockets;
using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Dtos;
using be_dotnet_ecommerce1.Repository;
using be_dotnet_ecommerce1.Repository.IReopsitory;
using be_dotnet_ecommerce1.Repository.IRepository;

namespace be_dotnet_ecommerce1.Service
{
  public class ProductService : IProductService
  {
    private readonly IProductRepository _repoProduct;

    public ProductService(IProductRepository repoProduct)
    {
      _repoProduct = repoProduct;
    }

    public async Task<PagedResultDTO<ProductFilterDTO>> getProductByFilter(FilterDTO dTO)
    {
      var conditions = new List<string>();
      var baseSql = @"FROM v_products_filter view";
      if (dTO.Filter != null)
      {
        foreach (var item in dTO.Filter)
        {
          var key = item.Key;
          if (key == "price")
          {
            if (item.Value.Count() == 2)
            {
              var min = item.Value[0];
              var max = item.Value[1];
              //conditions.Add($"v.price BETWEEN {min} AND {max}");
              conditions.Add($@"exists (
                select 1 
                from jsonb_array_elements(view.variant) as elem
                WHERE (elem->'price')::numeric BETWEEN {min} AND {max}
              )");
            }
          }
          else
          {
            var values = string.Join(",", item.Value.Select(v => $"'{v}'"));
            if (key == "brand")
              conditions.Add($"view.brand IN ({values})");
            else if (key == "category")
              conditions.Add($"view.categoryName  IN ({values})");
            else
              //conditions.Add($"v.valuevariant ->> '{key}' IN ({values})");
              conditions.Add($@" exists (
                select 1 
                from jsonb_array_elements(view.variant) as elem
                WHERE ((elem->'valuevariant')->>'{key}') IN ({values})
            )");
          }
        }
      }
      // nối where
      string wheresql = "";
      if (conditions.Any())
        wheresql = " where " + string.Join(" and ", conditions);
      // đếm số lượng sản phẩm
      var sqlcountProduct = $"select count(distinct view.id) as \"Value\" {baseSql} {wheresql}"; // cần phải có tên cột là value
      // lấy sản phẩm 
      var sqlData = $@"select distinct view.*
      {baseSql}
      {wheresql}
      order by view.id
      offset {(dTO.pageNumber - 1) * dTO.pageSize} rows 
      fetch next  {dTO.pageSize} rows only"; // rowns only

      // thực thi sql
      var totalCount = await _repoProduct.countProductBySql(sqlcountProduct); // đếm số lương sản phẩm
      
      var products = await _repoProduct.getProductBySql(sqlData); // lấy sản phẩm bằng sql

      // nếu không có sản phẩm nào
      if (totalCount == 0 || !products.Any())
      {
        return new PagedResultDTO<ProductFilterDTO>
        {
          Items = new List<ProductFilterDTO>(),
          TotalCount = 0,
          TotalPage = 0,
          PageNumber = dTO.pageNumber,
          PageSize = dTO.pageSize
        };
      }

      var totalPage = (int)Math.Ceiling(totalCount / (double)dTO.pageSize);
      return new PagedResultDTO<ProductFilterDTO>
      {
        Items = products,
        TotalCount = totalCount,
        TotalPage = totalPage,
        PageNumber = dTO.pageNumber,
        PageSize = dTO.pageSize
      };
    }
    public int getQuantityByIdCategory(int id)
    {
      var quantity = _repoProduct.getQuantityByIdCategory(id);
      return quantity;
    }
  }
}