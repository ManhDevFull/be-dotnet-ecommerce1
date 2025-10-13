namespace be_dotnet_ecommerce1.Repository.IRepository
{
    public interface IReviewRepository
    {
        public Task<int> getSumRatingByIdProduct(int id);
        public Task<int> getSumQuantityReviewByIdProduct(int id);
    }
}