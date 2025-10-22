namespace be_dotnet_ecommerce1.Repository.IRepository
{
    public interface IReviewRepository
    {
        public Task<Dictionary<int, int >> getSumRatingByIdsProduct(List<int> ids);
        public Task<Dictionary<int, int>> getSumQuantityReviewByIdProduct(List<int> ids);
    }
}