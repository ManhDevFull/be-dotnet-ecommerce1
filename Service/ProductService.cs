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
    private readonly IVariantRepository _repoVariant;
    private readonly ICategoryRepository _repoCategory;
    private readonly IReviewRepository _repoReview;
    private readonly IDiscountRepository _repoDiscount;

    public ProductService(IProductRepository repoProduct,
        IVariantRepository repoVariant,
        ICategoryRepository repoCategory,
        IReviewRepository repoReview,
        IDiscountRepository repoDiscount
    )
    {
      _repoProduct = repoProduct;
      _repoVariant = repoVariant;
      _repoCategory = repoCategory;
      _repoReview = repoReview;
      _repoDiscount = repoDiscount;
    }

    public async Task<PagedResultDTO<ProductFilterDTO>> getProductByFilter(FilterDTO dTO)
    {
      // var result = await _repo.getProductByFilter(dTO);
      //return result;
      var conditions = new List<string>();
      var baseSql = @"from product p
                JOIN category c ON (p.category = c.id)
                JOIN variant v on (p.id = v.product_id)";
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
              conditions.Add($"v.price BETWEEN {min} AND {max}");
            }
          }
          else
          {
            var values = string.Join(",", item.Value.Select(v => $"'{v}'"));
            if (key == "brand")
              conditions.Add($"p.brand IN ({values})");
            else if (key == "category")
              conditions.Add($"c.nameCategory IN ({values})");
            else
              conditions.Add($"v.valuevariant ->> '{key}' IN ({values})");
          }
        }
      }
      string wheresql = "";
      if (conditions.Any())
        wheresql = " where " + string.Join(" and ", conditions);
      // đếm số lượng sản phẩm
      var sqlcountProduct = $"select count(distinct p.id) as \"Value\" {baseSql} {wheresql}"; // cần phải có tên cột là value
      // lấy sản phẩm 
      var sqlData = $@"select distinct p.*
      {baseSql}
      {wheresql}
      order by p.id
      offset {(dTO.pageNumber - 1) * dTO.pageSize} rows // theo page (bỏ qua các sản phẩm từ page trước)
      fetch next  {dTO.pageSize} rows only"; // rowns only

      // thực thi sql
      var totalCount = await _repoProduct.countProductBySql(sqlcountProduct); // đếm số lương sản phẩm
      var productRaw = await _repoProduct.getProductBySql(sqlData); // lấy sản phẩm bằng sql

      // nếu không có sản phẩm nào
      if (totalCount == 0 || !productRaw.Any())
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

      // convert dữ liệu sang dto cho giao diện

      var categoryIds = productRaw.Select(p => p.categoryId).Distinct().ToList();      // lấy ra id category
      var productIds = productRaw.Select(p => p.id).Distinct().ToList(); // lấy ra id product từ product raw

      var products = new List<ProductFilterDTO>(); // tạo danh sách trả về sản phẩm đã lọc
      var discountTask = await _repoDiscount.getDiscountByIdProducts(productIds); // lấy task xử lý ở responstory
      var ratingTask = await _repoReview.getSumRatingByIdsProduct(productIds);
      var orderTask = await _repoReview.getSumQuantityReviewByIdProduct(productIds);
      var categoryTask = await _repoCategory.getCategoryByProductIds(productIds);
      var variantTask = await _repoVariant.getVariantByIdProducts(productIds);
 
      foreach (var p in productRaw)
      {
        discountTask.TryGetValue(p.id, out var discount);
        ratingTask.TryGetValue(p.id, out var rating);
        orderTask.TryGetValue(p.id, out var order);
        var category = categoryTask.FirstOrDefault(c => c.id == p.categoryId);
        var variant = variantTask.Where(v => v.productid == p.id).Distinct();
        products.Add(new ProductFilterDTO
        {
          id = p.id,
          name = p.nameproduct,
          description = p.description,
          brand = p.brand,
          categoryId = p.categoryId,
          //categoryName = p.Category.namecategory,
          //categoryName = p.Category?.namecategory ?? "Unknown",
          categoryName = category?.namecategory ?? "Unknown",
          imgUrls = p.imageurls,
          variant = variant.Select(v => new VariantDTO
          {
            id = v.id,
            valuevariant = v.valuevariant,
            stock = v.stock,
            inputprice = v.inputprice,
            price = v.price,
            createdate = v.createdate,
            updatedate = v.updatedate
          }).ToArray(),
          discount = discount,
          rating = rating,
          order = order
        });
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