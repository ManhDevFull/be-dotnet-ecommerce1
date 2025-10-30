using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using be_dotnet_ecommerce1.Model;
using dotnet.Dtos;

namespace dotnet.Service.IService
{
  public interface IUserService
  {
    public List<UserDTO> getUsers();
    Task<Account?> GetUserProfileByIdAsync(int userId);
    Task<Account?> UpdateProfileAsync(int userId, UserProfileUpdateDTO dto);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);

    Task<bool> UpdateAvatarUrlAsync(int userId, string avatarUrl);
  }
}