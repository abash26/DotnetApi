using System.Security.Cryptography;
using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Helpers;
using DotnetApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthEFController : ControllerBase
{
  private readonly IAuthRepository _authRepository;
  private readonly IUserRepository _userRepository;
  private readonly AuthHelper _authHelper;

  public AuthEFController(IConfiguration config, IAuthRepository authRepository, IUserRepository userRepository)
  {
    _authRepository = authRepository;
    _userRepository = userRepository;
    _authHelper = new AuthHelper(config);
  }

  [AllowAnonymous]
  [HttpPost("Register")]
  public async Task<IActionResult> Register(UserForRegistrationDto userToRegister)
  {
    if (userToRegister.Password != userToRegister.PasswordConfirm)
    {
      return BadRequest("Passwords do not match");
    }
    if (await _authRepository.UserExists(userToRegister.Email))
    {
      return BadRequest("User with this email already exists");
    }

    var passwordSalt = new byte[128 / 8];
    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
    {
      rng.GetNonZeroBytes(passwordSalt);
    }

    var passwordHash = _authHelper.GetPasswordHash(userToRegister.Password, passwordSalt);

    var authData = new Auth
    {
      Email = userToRegister.Email,
      PasswordHash = passwordHash,
      PasswordSalt = passwordSalt
    };

    var user = new User
    {
      FirstName = userToRegister.FirstName,
      LastName = userToRegister.LastName,
      Email = userToRegister.Email,
      Gender = userToRegister.Gender,
      Active = true
    };

    await _authRepository.Register(user, authData);

    if (await _authRepository.SaveAll())
    {
      return Ok();
    }
    return BadRequest("Failed to register user");
  }

  [AllowAnonymous]
  [HttpPost("Login")]
  public async Task<IActionResult> Login(UserForLoginDto userForLogin)
  {
    var user = await _authRepository.GetUserByEmailAsync(userForLogin.Email);

    if (user == null)
    {
      return NotFound("User not found.");
    }

    var passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, user.PasswordSalt);
    if (!passwordHash.SequenceEqual(user.PasswordHash))
    {
      return StatusCode(401, "Incorrect password");
    }
    return Ok();
  }

  [HttpGet("RefreshToken")]
  public IActionResult RefreshToken()
  {
    var userIdString = User.FindFirst("userId")?.Value;

    if (string.IsNullOrEmpty(userIdString))
    {
      return BadRequest("User ID is missing or invalid.");
    }

    if (!int.TryParse(userIdString, out int userId))
    {
      return BadRequest("User ID is invalid.");
    }

    var user = _userRepository.GetSingleUser(userId);
    if (user == null)
    {
      return NotFound("User not found.");
    }

    var token = _authHelper.CreateToken(user.UserId);
    return Ok(new Dictionary<string, string> { { "token", token } });
  }
}

