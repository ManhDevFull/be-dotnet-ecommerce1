using be_dotnet_ecommerce1.Model;
using dotnet.Dtos;
using dotnet.Dtos.admin;
namespace dotnet.Repository.IRepository
{
    public interface IUserReponsitory
    {
        public List<UserDTO> getUserAdmin();
        public Task<Account?> GetByIdAsync(int userId);
        void Update(Account user);
        Task SaveChangesAsync();
       public List<UserAdminDTO> getUserAdmin();
  }
}