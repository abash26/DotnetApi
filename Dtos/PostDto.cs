namespace DotnetApi.Dtos;

public partial class PostDto
{
  public int? PostId { get; set; }
  public int? UserId { get; set; }
  public string PostTitle { get; set; } = "";
  public string PostContent { get; set; } = "";
}