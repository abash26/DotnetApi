namespace DotnetApi.Models;

public partial class Auth
{
  public string Email { get; set; } = "";
  public byte[] PasswordHash { get; set; } = [];
  public byte[] PasswordSalt { get; set; } = [];
}