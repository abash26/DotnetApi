using DotnetApi.Data;
using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserSalaryController : ControllerBase
{
  DataContextDapper _dapper;

  public UserSalaryController(IConfiguration config)
  {
    _dapper = new DataContextDapper(config);
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

