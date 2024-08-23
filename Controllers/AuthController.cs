using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetApi.Data;
using DotnetApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
  private readonly DataContextDapper _dapper;
  private readonly IConfiguration _config;

  public AuthController(IConfiguration config)
  {
    _dapper = new DataContextDapper(config);
    _config = config;
  }

  [AllowAnonymous]
  [HttpPost("Register")]
  public IActionResult Register(UserForRegistrationDto userToRegister)
  {
    if (userToRegister.Password == userToRegister.PasswordConfirm)
    {
      var sqlUserExists = @"SELECT Email FROM TutorialAppSchema.Auth
        WHERE Email = '" + userToRegister.Email + "'";

      var existingUsers = _dapper.LoadData<string>(sqlUserExists);

      if (existingUsers.Count() == 0)
      {
        var passwordSalt = new byte[128 / 8];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
          rng.GetNonZeroBytes(passwordSalt);
        }

        var passwordHash = GetPasswordHash(userToRegister.Password, passwordSalt);

        string sqlAddAuth = @"INSERT INTO TutorialAppSchema.Auth (
          [Email],
          [PasswordHash],
          [PasswordSalt]
        ) VALUES ('" + userToRegister.Email +
          "', @PasswordHash, @PasswordSalt)";

        var sqlParameters = new List<SqlParameter>();
        var passwordSaltParam = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary)
        {
          Value = passwordSalt
        };

        var passwordHashParam = new SqlParameter("@PasswordHash", SqlDbType.VarBinary)
        {
          Value = passwordHash
        };

        sqlParameters.Add(passwordSaltParam);
        sqlParameters.Add(passwordHashParam);
        if (_dapper.ExecuteSqlWithParam(sqlAddAuth, sqlParameters))
        {
          string sqlAddUser = @"INSERT INTO TutorialAppSchema.Users(
            [FirstName],
            [LastName],
            [Email],
            [Gender],
            [Active]
          ) VALUES (" +
              "'" + userToRegister.FirstName +
              "', '" + userToRegister.LastName +
              "', '" + userToRegister.Email +
              "', '" + userToRegister.Gender +
              "', 1)";
          if (_dapper.ExecuteSql(sqlAddUser))
          {
            return Ok();
          }
          throw new Exception("Failed to add user");
        }
        throw new Exception("Failed to register user");
      }
      throw new Exception("User with this email already exists");
    }
    throw new Exception("Passwords do not match");
  }

  [AllowAnonymous]
  [HttpPost("Login")]
  public IActionResult Login(UserForLoginDto userForLogin)
  {
    var sqlForHashAndSalt = @"SELECT [PasswordHash], [PasswordSalt] FROM TutorialAppSchema.Auth
         WHERE Email = '" + userForLogin.Email + "'";
    var userForConfirmation = _dapper.LoadDataSingle<UserForLoginConfirmDto>(sqlForHashAndSalt);

    var passwordHash = GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

    for (int i = 0; i < passwordHash.Length; i++)
    {
      if (passwordHash[i] != userForConfirmation.PasswordHash[i])
      {
        return StatusCode(401, "Incorrect password");
      }
    }

    var userIdsql = @"SELECT [userId] FROM TutorialAppSchema.Users
         WHERE Email = '" + userForLogin.Email + "'";

    var userId = _dapper.LoadDataSingle<int>(userIdsql);

    return Ok(new Dictionary<string, string> {
      {"token", CreateToken(userId)}
    });
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

  [HttpGet("RefreshToken")]
  public IActionResult RefreshToken()
  {
    var userId = User.FindFirst("userId")?.Value + "";
    var userIdSql = "SELECT UserId FROM TutorialAppSchema.Users WHERE UserId = " + userId;
    var userIdFromDb = _dapper.LoadDataSingle<int>(userIdSql);

    return Ok(new Dictionary<string, string> {
      {"token", CreateToken(userIdFromDb)}
    });
  }

  private string CreateToken(int userId)
  {
    var claims = new Claim[]
    {
      new Claim("userId", userId.ToString())
    };

    string? tokenKeyString = _config.GetSection("AppSettings:Token").Value;

    var tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKeyString ?? ""));

    var credentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha256);

    var descriptor = new SecurityTokenDescriptor()
    {
      Subject = new ClaimsIdentity(claims),
      SigningCredentials = credentials,
      Expires = DateTime.Now.AddDays(1)
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(descriptor);

    return tokenHandler.WriteToken(token);
  }
}

