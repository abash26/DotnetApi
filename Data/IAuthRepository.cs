using DotnetApi.Models;

namespace DotnetApi.Data;

public interface IAuthRepository
{
  Task<bool> UserExists(string email);
  Task<Auth?> GetUserByEmailAsync(string email);
  Task<bool> SaveAll();
  Task Register(User user, Auth authData);
}