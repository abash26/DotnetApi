using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using DotnetApi.Data;
using DotnetApi.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace DotnetApi.Helpers;

public class AuthHelper
{
  private readonly IConfiguration _config;
  private readonly DataContextDapper _dapper;
  public AuthHelper(IConfiguration config)
  {
    _config = config;
    _dapper = new DataContextDapper(config);
  }
  public byte[] GetPasswordHash(string password, byte[] passwordSalt)
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
  public string CreateToken(int userId)
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

  public bool SetPassword(UserForLoginDto user)
  {
    var passwordSalt = new byte[128 / 8];
    using (var rng = RandomNumberGenerator.Create())
    {
      rng.GetNonZeroBytes(passwordSalt);
    }
    var passwordHash = GetPasswordHash(user.Password, passwordSalt);

    var registrationParams = new DynamicParameters(new
    {
      user.Email,
      PasswordHash = passwordHash,
      PasswordSalt = passwordSalt
    });

    string sqlRegistration = @"EXEC TutorialAppSchema.spRegistration_Upsert @Email, @PasswordHash, @PasswordSalt";

    return _dapper.ExecuteSql(sqlRegistration, registrationParams);
  }
}