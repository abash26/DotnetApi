namespace DotnetApi.Dtos;

public partial class UserForLoginConfirmDto
{
  public byte[] PasswordHash { get; set; } = [];
  public byte[] PasswordSalt { get; set; } = [];
}