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
      var parameters = new DynamicParameters();

      parameters.Add("@UserId", userId);
      parameters.Add("@Active", isActive);

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
        @FirstName = '" + user.FirstName +
        "', @LastName = '" + user.LastName +
        "', @Email = '" + user.Email +
        "', @Gender = '" + user.Gender +
        "', @Active = '" + user.Active +
        "', @JobTitle = '" + user.JobTitle +
        "', @Department = '" + user.Department +
        "', @Salary = '" + user.Salary +
        "', @UserId = " + user.UserId;

    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }
    throw new Exception("Failed to update user");
  }

  [HttpDelete("DeleteUser/{userId}")]
  public IActionResult DeleteUser(int userId)
  {
    string sql = @"EXEC TutorialAppSchema.spUser_Delete 
      @UserId = " + userId.ToString();
    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }
    throw new Exception("Failed to delete user");
  }
}

