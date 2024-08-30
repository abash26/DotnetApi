using Dapper;
using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
  private readonly DataContextDapper _dapper;

  public PostController(IConfiguration config)
  {
    _dapper = new DataContextDapper(config);
  }

  [HttpGet("GetPosts")]
  public ActionResult<IEnumerable<Post>> GetPosts(
    [FromQuery] int? postId = null,
    [FromQuery] int? userId = null,
    [FromQuery] string? searchValue = null)
  {
    try
    {
      string sql = "EXEC TutorialAppSchema.spPosts_Get @UserId, @SearchValue, @PostId";
      var parameters = new DynamicParameters();

      parameters.Add("@UserId", userId);
      parameters.Add("@SearchValue", searchValue);
      parameters.Add("@PostId", postId);

      var posts = _dapper.LoadData<Post>(sql, parameters);
      return Ok(posts);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      return StatusCode(500, "Internal Server Error. Please try again later.");
    }
  }

  [HttpGet("GetMyPosts")]
  public IEnumerable<Post> GetMyPosts()
  {
    string sql = @"SELECT 
        [PostId],
        [UserId],
        [PostTitle],
        [PostContent],
        [PostCreated],
        [PostUpdated]
      FROM TutorialAppSchema.Posts
      WHERE UserId = " + User.FindFirst("userId")?.Value;

    var posts = _dapper.LoadData<Post>(sql);
    return posts;
  }

  [HttpPut("EditPost")]
  public IActionResult EditPost(PostToEditDto post)
  {
    string sql = @"
      UPDATE TutorialAppSchema.Posts
        SET [PostTitle] = '" + post.PostTitle +
        "', [PostContent] = '" + post.PostContent +
        "', [PostUpdated] = GETDATE() WHERE PostId = " + post.PostId +
        " AND UserId = " + User.FindFirst("userId")?.Value;
    Console.WriteLine(sql);

    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }
    throw new Exception("Failed to update post");
  }

  [HttpPost("AddPost")]
  public IActionResult AddPost(PostToAddDto post)
  {
    string sql = @"INSERT INTO TutorialAppSchema.Posts(
        [UserId],
        [PostTitle],
        [PostContent],
        [PostCreated],
        [PostUpdated]
      ) VALUES (" + User.FindFirst("userId")?.Value +
        ", '" + post.PostTitle +
        "', '" + post.PostContent +
        "', GETDATE(), GETDATE())";

    Console.WriteLine(sql);

    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }
    throw new Exception("Failed to add post");
  }

  [HttpDelete("DeletePost/{postId}")]
  public IActionResult DeletePost(int postId)
  {
    string sql = @"
      DELETE FROM TutorialAppSchema.Posts
        WHERE PostId = " + postId.ToString() +
        "AND UserId = " + User.FindFirst("userId")?.Value;

    if (_dapper.ExecuteSql(sql))
    {
      return Ok();
    }
    throw new Exception("Failed to delete post");
  }
}

