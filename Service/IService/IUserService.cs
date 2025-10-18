using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet.Dtos.admin;

namespace dotnet.Service.IService
{
  public interface IUserService
  {
    public List<UserAdminDTO> getUsers();
  }
}