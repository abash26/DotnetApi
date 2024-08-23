using System.Security.Cryptography;
using System.Text;
using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthEFController : ControllerBase
{
  private readonly IAuthRepository _authRepository;
  private readonly IConfiguration _config;

  public AuthEFController(IConfiguration config, IAuthRepository authRepository)
  {
    _authRepository = authRepository;
    _config = config;
  }

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

    var passwordHash = GetPasswordHash(userToRegister.Password, passwordSalt);

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

  [HttpPost("Login")]
  public async Task<IActionResult> Login(UserForLoginDto userForLogin)
  {
    var user = await _authRepository.GetUserByEmailAsync(userForLogin.Email);

    if (user == null)
    {
      return NotFound("User not found.");
    }

    var passwordHash = GetPasswordHash(userForLogin.Password, user.PasswordSalt);
    if (!passwordHash.SequenceEqual(user.PasswordHash))
    {
      return StatusCode(401, "Incorrect password");
    }
    return Ok();
  }

  private byte[] GetPasswordHash(string password, byte[] passwordSalt)
  {
    string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value +
          Convert.ToBase64String(passwordSalt);

    var passwordHash = KeyDerivation.Pbkdf2(
      password: password,
      salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
      prf: KeyDerivationPrf.HMACSHA256,
      iterationCount: 10000,
      numBytesRequested: 256 / 8
    );

    return passwordHash;
  }
}

