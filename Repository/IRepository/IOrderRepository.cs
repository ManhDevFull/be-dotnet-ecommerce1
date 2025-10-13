namespace be_dotnet_ecommerce1.Repository.IRepository
{
    public interface IOrderRepository
    {
        public Task<int> getQuantityOrderByIdProduct(int id);
    }
}