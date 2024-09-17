using Dapper;
using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
  private readonly DataContextDapper _dapper;
  private readonly AuthHelper _authHelper;

  public AuthController(IConfiguration config)
  {
    _dapper = new DataContextDapper(config);
    _authHelper = new AuthHelper(config);
  }

  [AllowAnonymous]
  [HttpPost("Register")]
  public IActionResult Register([FromBody] UserForRegistrationDto userToRegister)
  {
    if (userToRegister.Password != userToRegister.PasswordConfirm)
    {
      return BadRequest("Passwords do not match");
    }

    var loginParams = new DynamicParameters(new { userToRegister.Email });
    string sqlUserExists = @"EXEC TutorialAppSchema.spAuthUser_Get @Email";

    var existingUsers = _dapper.LoadData<string>(sqlUserExists, loginParams);

    if (existingUsers.Count() != 0)
    {
      return BadRequest("User with this email already exists");
    }

    var loginDto = new UserForLoginDto
    {
      Email = userToRegister.Email,
      Password = userToRegister.Password
    };

    if (!_authHelper.SetPassword(loginDto))
    {
      return StatusCode(500, "Failed to register user");
    }

    var userParams = new DynamicParameters(new
    {
      userToRegister.FirstName,
      userToRegister.LastName,
      userToRegister.Email,
      userToRegister.Gender,
      userToRegister.JobTitle,
      userToRegister.Department,
      userToRegister.Salary,
      Active = 1,
    });

    string sqlAddUser = @"EXEC TutorialAppSchema.spUser_Upsert 
        @FirstName, @LastName, @Email, @Gender, @JobTitle, @Department, @Salary, @Active";

    if (!_dapper.ExecuteSql(sqlAddUser, userParams))
    {
      return StatusCode(500, "Failed to update user");
    }

    return Ok("User registered successfully.");
  }

  [HttpPut("ResetPassword")]
  public IActionResult ResetPassword([FromBody] UserForLoginDto userToSetPassword)
  {
    if (!_authHelper.SetPassword(userToSetPassword))
    {
      return StatusCode(500, "Failed to register user");
    }
    return Ok("Password updated successfully.");
  }

  [AllowAnonymous]
  [HttpPost("Login")]
  public IActionResult Login(UserForLoginDto userForLogin)
  {
    var loginParams = new DynamicParameters();
    loginParams.Add("@Email", userForLogin.Email);

    string sqlForHashAndSalt = @"EXEC TutorialAppSchema.spLoginConfirmation_Get @Email";

    var userForConfirmation = _dapper.LoadDataSingle<UserForLoginConfirmDto>(sqlForHashAndSalt, loginParams);

    var passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

    for (int i = 0; i < passwordHash.Length; i++)
    {
      if (passwordHash[i] != userForConfirmation.PasswordHash[i])
      {
        return StatusCode(401, "Incorrect password");
      }
    }

    string userIdsql = @"EXEC TutorialAppSchema.spUser_Get @Email";

    var userId = _dapper.LoadDataSingle<int>(userIdsql, loginParams);

    return Ok(new Dictionary<string, string> {
      {"token", _authHelper.CreateToken(userId)}
    });
  }

  [HttpGet("RefreshToken")]
  public IActionResult RefreshToken()
  {
    var userIdString = User.FindFirst("userId")?.Value + "";
    if (string.IsNullOrEmpty(userIdString))
    {
      return BadRequest("Authentication failed. User ID is missing.");
    }

    var userParams = new DynamicParameters(new { UserId = userIdString });

    var userIdFromDb = _dapper.LoadDataSingle<int>(@"EXEC TutorialAppSchema.spUser_Get @UserId", userParams);

    return Ok(new Dictionary<string, string> {
      {"token", _authHelper.CreateToken(userIdFromDb)}
    });
  }

}

