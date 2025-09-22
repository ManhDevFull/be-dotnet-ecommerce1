using be_dotnet_ecommerce1.Model;

namespace be_dotnet_ecommerce1.Repository.IReopsitory
{
    public interface ICategoryRepository
    {
        public List<Category> getParentById(int? id);
    }
}