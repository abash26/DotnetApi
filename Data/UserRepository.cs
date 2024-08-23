using DotnetApi.Models;

namespace DotnetApi.Data;

public class UserRepository : IUserRepository
{
  DataContextEF _entityFramework;
  public UserRepository(IConfiguration config)
  {
    _entityFramework = new DataContextEF(config);
  }

  public bool SaveChanges()
  {
    return _entityFramework.SaveChanges() > 0;
  }
  public void AddEntity<T>(T entityToAdd)
  {
    if (entityToAdd != null)
    {
      _entityFramework.Add(entityToAdd);
    }
  }

  public void RemoveEntity<T>(T entityToRemove)
  {
    if (entityToRemove != null)
    {
      _entityFramework.Remove(entityToRemove);
    }
  }

  public IEnumerable<User> GetUsers()
  {
    return _entityFramework.Users.ToList();
  }

  public User GetSingleUser(int userId)
  {
    var user = _entityFramework.Users
      .Where(u => u.UserId == userId)
      .FirstOrDefault();

    if (user != null)
    {
      return user;
    };
    throw new Exception("Failed to get user");
  }

  public UserJobInfo GetSingleUserJobInfo(int userId)
  {
    var userJobInfo = _entityFramework.UserJobInfo
      .Where(u => u.UserId == userId)
      .FirstOrDefault();

    if (userJobInfo != null)
    {
      return userJobInfo;
    };
    throw new Exception("Failed to get user's job info");
  }

  public UserSalary GetSingleUserSalary(int userId)
  {
    var userSalary = _entityFramework.UserSalary
      .Where(u => u.UserId == userId).FirstOrDefault();
    if (userSalary != null)
    {
      return userSalary;
    }
    throw new Exception("Failed to get user's salary");
  }
}