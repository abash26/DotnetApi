using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
  DataContextDapper _dapper;

  public UserController(IConfiguration config)
  {
    _dapper = new DataContextDapper(config);
  }

  [HttpGet("GetUsers")]
  public IEnumerable<User> GetUsers()
  {
    string sql = @"
      SELECT [UserId],
        [FirstName],
        [LastName],
        [Email],
        [Gender],
        [Active] 
      FROM TutorialAppSchema.Users";

    var users = _dapper.LoadData<User>(sql);
    return users;
  }

  [HttpGet("GetSingleUser/{userId}")]
  public User GetSingleUser(int userId)
  {
    string sql = @"
      SELECT [UserId],
        [FirstName],
        [LastName],
        [Email],
        [Gender],
        [Active] 
      FROM TutorialAppSchema.Users
        WHERE UserId = " + userId.ToString();

    var user = _dapper.LoadDataSingle<User>(sql);
    return user;
  }

  [HttpPut("EditUser")]
  public IActionResult EditUser(User user)
  {
    string sql = @"
      UPDATE TutorialAppSchema.Users
        SET [FirstName] = '" + user.FirstName +
        "', [LastName] = '" + user.LastName +
        "', [Email] = '" + user.Email +
        "', [Gender] = '" + user.Gender +
        "', [Active] = '" + user.Active +
        "' WHERE UserId = " + user.UserId;

    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }
    throw new Exception("Failed to update user");
  }

  [HttpPost("AddUser")]
  public IActionResult AddUser(UserToAddDto user)
  {
    string sql = @"INSERT INTO TutorialAppSchema.Users(
        [FirstName],
        [LastName],
        [Email],
        [Gender],
        [Active]
      ) VALUES (" +
        "'" + user.FirstName +
        "', '" + user.LastName +
        "', '" + user.Email +
        "', '" + user.Gender +
        "', '" + user.Active +
      "')";

    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }
    throw new Exception("Failed to add user");
  }

  [HttpDelete("DeleteUser/{userId}")]
  public IActionResult DeleteUser(int userId)
  {
    string sql = @"
      DELETE FROM TutorialAppSchema.Users
        WHERE UserId = " + userId.ToString();

    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }
    throw new Exception("Failed to delete user");
  }
}

