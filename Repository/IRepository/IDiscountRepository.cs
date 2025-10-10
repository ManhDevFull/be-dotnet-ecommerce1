using dotnet.Model;

namespace be_dotnet_ecommerce1.Repository.IRepository
{
    public interface IDiscountRepository
    {
        public Task<Discount> getDiscountByIdProduct(int id);
    }
}