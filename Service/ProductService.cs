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

        public async Task<List<ProductFilterDTO>> getProductByFilter(FilterDTO dTO)
        {
            // var result = await _repo.getProductByFilter(dTO);
            //return result;
            var conditions = new List<string>();
            var sql = @"select distinct p.* from product p
                JOIN category c ON 
                (p.category = c.id)
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
            if (conditions.Any())
                sql += " Where " + string.Join(" AND ", conditions);

            var productRaw = await _repoProduct.getProductBySql(sql);
            var products = new List<ProductFilterDTO>();
            var categories = await _repoCategory.getAllCategory();
            foreach (var p in productRaw)
            {
                var varaints = await _repoVariant.GetVariantByIdProduct(p.id);
                products.Add(new ProductFilterDTO
                {
                    id = p.id,
                    name = p.nameproduct,
                    description = p.description,
                    brand = p.brand,
                    categoryId = p.categoryId,
                    //categoryName = p.Category.namecategory,
                    //categoryName = p.Category?.namecategory ?? "Unknown",
                    categoryName = categories
                                .Where(c => c.id == p.categoryId)
                                .Select(c => c.namecategory)
                                .FirstOrDefault() ?? "Unknown",
                    imgUrls = p.imageurls,
                    variant = varaints.Select(v => new VariantDTO
                    {
                        id = v.id,
                        valuevariant = v.valuevariant,
                        stock = v.stock,
                        inputprice = v.inputprice,
                        price = v.price,
                        createdate = v.createdate,
                        updatedate = v.updatedate
                    }).ToArray(),
                    discount = await _repoDiscount.getDiscountByIdProduct(p.id),
                    rating = await _repoReview.getSumRatingByIdProduct(p.id),
                    order = await _repoReview.getSumQuantityReviewByIdProduct(p.id)
                });
            }
            return products;
        }
        public int getQuantityByIdCategory(int id)
        {
            var quantity = _repoProduct.getQuantityByIdCategory(id);
            return quantity;
        }
    }
}