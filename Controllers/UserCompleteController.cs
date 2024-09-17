using Dapper;
using DotnetApi.Data;
using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{
  DataContextDapper _dapper;

  public UserCompleteController(IConfiguration config)
  {
    _dapper = new DataContextDapper(config);
  }

  [HttpGet("GetUsers")]
  public ActionResult<IEnumerable<UserComplete>> GetUsers([FromQuery] int? userId = null, [FromQuery] bool? isActive = null)
  {
    try
    {
      string sql = "EXEC TutorialAppSchema.spUsers_Get @UserId, @Active";
      var parameters = new DynamicParameters(new
      {
        userId,
        isActive
      });

      var users = _dapper.LoadData<UserComplete>(sql, parameters);
      return Ok(users);
    }
    catch (Exception ex)
    {
      return StatusCode(500, "Internal Server Error. Please try again later.");
    }
  }

  [HttpPut("UpsertUser")]
  public IActionResult EditUser(UserComplete user)
  {
    string sql = @"EXEC TutorialAppSchema.spUser_Upsert
        @FirstName,
        @LastName,
        @Email,
        @Gender,
        @Active,
        @JobTitle,
        @Department,
        @Salary,
        @UserId";

    var parameters = new DynamicParameters();
    parameters.Add("FirstName", user.FirstName);
    parameters.Add("LastName", user.LastName);
    parameters.Add("Email", user.Email);
    parameters.Add("Gender", user.Gender);
    parameters.Add("Active", user.Active);
    parameters.Add("JobTitle", user.JobTitle);
    parameters.Add("Department", user.Department);
    parameters.Add("Salary", user.Salary);
    parameters.Add("UserId", user.UserId);

    if (_dapper.ExecuteSql(sql, parameters))
    {
      return Ok();
    }
    throw new Exception("Failed to update user");
  }

  [HttpDelete("DeleteUser/{userId}")]
  public IActionResult DeleteUser(int userId)
  {
    string sql = @"EXEC TutorialAppSchema.spUser_Delete @UserId";
    var parameters = new DynamicParameters();
    parameters.Add("UserId", userId.ToString());

    if (_dapper.ExecuteSql(sql, parameters))
    {
      return Ok();
    }
    throw new Exception("Failed to delete user");
  }
}

