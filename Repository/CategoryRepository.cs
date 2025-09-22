using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Model;
using be_dotnet_ecommerce1.Repository.IReopsitory;

namespace be_dotnet_ecommerce1.Repository
{

    public class CategoryRepository : ICategoryRepository
    {
        private readonly ConnectData _connect;
        public CategoryRepository(ConnectData connect) {
            _connect = connect;
        }
        public List<Category> getParentById(int? id)
        {
            return _connect.categories.Where(c => c.parent_id == id).ToList();
        }

    }
}