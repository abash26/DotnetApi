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

  [HttpGet("GetSingleUserJobInfo/{userId}")]
  public UserJobInfo GetSingleUserJobInfo(int userId)
  {
    var sql = @"
      SELECT [UserId], [JobTitle], [Department] 
      FROM TutorialAppSchema.UserJobInfo
      WHERE UserId = " + userId.ToString();

    var userJobInfo = _dapper.LoadDataSingle<UserJobInfo>(sql);
    return userJobInfo;
  }

  [HttpPost("AddUserJobInfo")]
  public IActionResult AddUserJobInfo(UserJobInfo userJobInfo)
  {
    var sql = @"INSERT INTO TutorialAppSchema.UserJobInfo(
        [UserId],
        [JobTitle],
        [Department]
      ) VALUES(" +
      "'" + userJobInfo.UserId +
      "', '" + userJobInfo.JobTitle +
      "', '" + userJobInfo.Department +
    "')";

    Console.WriteLine(sql);

    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }
    throw new Exception("Failed to add user's salary");
  }

  [HttpPut("EditUserJobInfo")]
  public IActionResult EditUserJobInfo(UserJobInfo userJobInfo)
  {
    string sql = @"
      UPDATE TutorialAppSchema.UserJobInfo
        SET [UserId] = '" + userJobInfo.UserId +
        "', [JobTitle] = '" + userJobInfo.JobTitle +
        "', [Department] = '" + userJobInfo.Department +
        "' WHERE UserId = " + userJobInfo.UserId;

    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }
    throw new Exception("Failed to update user's job info");
  }

  [HttpDelete("DeleteUserJobInfo/{userId}")]
  public IActionResult DeleteUserJobInfo(int userId)
  {
    var sql = @"
      DELETE FROM TutorialAppSchema.UserJobInfo
      WHERE UserId = " + userId.ToString();
    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }
    throw new Exception("Failed to delete user's job info");
  }

  [HttpGet("GetSingleUserSalary/{userId}")]
  public UserSalary GetSingleUserSalary(int userId)
  {
    string sql = @"
      SELECT [UserId], [Salary]
      FROM TutorialAppSchema.UserSalary
        WHERE UserId = " + userId.ToString();

    var userSalary = _dapper.LoadDataSingle<UserSalary>(sql);
    return userSalary;
  }

  [HttpPut("EditSalary")]
  public IActionResult EditSalary(UserSalary userSalary)
  {
    string sql = @"
      UPDATE TutorialAppSchema.UserSalary
        SET [Salary] = '" + userSalary.Salary +
        "' WHERE UserId = " + userSalary.UserId;

    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }
    throw new Exception("Failed to edit user's salary");
  }

  [HttpPost("AddUserSalary")]
  public IActionResult AddUserSalary(UserSalary userSalary)
  {
    string sql = @"INSERT INTO TutorialAppSchema.UserSalary(
        [UserId],
        [Salary]
      ) VALUES (" + userSalary.UserId + ", " + userSalary.Salary + ")";

    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }
    throw new Exception("Failed to add user's salary");
  }

  [HttpDelete("DeleteUserSalary/{userId}")]
  public IActionResult DeleteUserSalary(int userId)
  {
    string sql = @"DELETE FROM TutorialAppSchema.UserSalary WHERE UserId = " + userId.ToString();

    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }
    throw new Exception("Failed to delete user's salary");
  }
}

