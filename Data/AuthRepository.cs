using DotnetApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetApi.Data;
public class AuthRepository : IAuthRepository
{
  DataContextEF _entityFramework;
  public AuthRepository(IConfiguration config)
  {
    _entityFramework = new DataContextEF(config);
  }

  public async Task<bool> SaveAll()
  {
    return await _entityFramework.SaveChangesAsync() > 0;
  }

  public async Task<bool> UserExists(string email)
  {
    return await _entityFramework.Auth.AnyAsync(a => a.Email == email);
  }

  public async Task<Auth?> GetUserByEmailAsync(string email)
  {
    return await _entityFramework.Auth.FirstOrDefaultAsync(u => u.Email == email);
  }

  public async Task Register(User user, Auth authData)
  {
    await _entityFramework.Users.AddAsync(user);
    await _entityFramework.Auth.AddAsync(authData);
  }


}