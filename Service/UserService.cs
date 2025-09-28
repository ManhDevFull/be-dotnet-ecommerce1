using dotnet.Dtos;
using dotnet.Repository.IRepository;
using dotnet.Service.IService;

namespace dotnet.Service
{
    public class UserService : IUserService
    {
    private readonly IUserReponsitory _repo;
    public UserService(IUserReponsitory repo){
      _repo = repo;
    }
    public List<UserDTO> getUsers(){
      var list = _repo.getUserAdmin();
      return list;
    }
  }
}