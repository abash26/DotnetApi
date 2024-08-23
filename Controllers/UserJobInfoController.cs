using DotnetApi.Data;
using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserJobInfoController : ControllerBase
{
  DataContextDapper _dapper;

  public UserJobInfoController(IConfiguration config)
  {
    _dapper = new DataContextDapper(config);
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
}
