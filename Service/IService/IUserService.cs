using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet.Dtos;

namespace dotnet.Service.IService
{
  public interface IUserService
  {
    public List<UserDTO> getUsers();
  }
}