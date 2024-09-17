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
    var parameters = new DynamicParameters();
    parameters.Add("@UserId", User.FindFirst("userId")?.Value);
    string sql = @"EXEC TutorialAppSchema.spPosts_Get @UserId";

    var posts = _dapper.LoadData<Post>(sql, parameters);
    return posts;
  }

  [HttpPost("CreatePost")]
  public IActionResult CreatePost([FromBody] PostDto post)
  {
    try
    {
      var parameters = new DynamicParameters();

      parameters.Add("@UserId", User.FindFirst("userId")?.Value);
      parameters.Add("@PostId", post.PostId ?? default(int?));
      parameters.Add("@PostTitle", post.PostTitle);
      parameters.Add("@PostContent", post.PostContent);

      string sql = @"EXEC TutorialAppSchema.spPosts_Upsert @UserId, @PostId, @PostTitle, @PostContent";

      _dapper.LoadData<Post>(sql, parameters);
      return Ok("Post created successfully!");
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.ToString());
      return StatusCode(500, "Internal Server Error. Please try again later.");
    }
  }

  [HttpPut("UpdatePost/{postId}")]
  public IActionResult CreatePost(int postId, [FromBody] PostDto post)
  {
    try
    {
      var parameters = new DynamicParameters();
      parameters.Add("@UserId", User.FindFirst("userId")?.Value);
      parameters.Add("@PostId", postId);
      parameters.Add("@PostTitle", post.PostTitle);
      parameters.Add("@PostContent", post.PostContent);

      string sql = @"EXEC TutorialAppSchema.spPosts_Upsert @UserId, @PostId, @PostTitle, @PostContent";

      _dapper.LoadData<Post>(sql, parameters);
      return Ok("Post updated successfully!");
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.ToString());
      return StatusCode(500, "Internal Server Error. Please try again later.");
    }
  }

  [HttpDelete("DeletePost/{postId}")]
  public IActionResult DeletePost(int postId)
  {
    try
    {
      var parameters = new DynamicParameters();
      parameters.Add("@UserId", User.FindFirst("userId")?.Value);
      parameters.Add("@PostId", postId);

      string sql = @"EXEC TutorialAppSchema.spPost_Delete @PostId, @UserId";

      if (_dapper.ExecuteSql(sql, parameters))
      {
        return Ok();
      }
      else
      {
        return NotFound("Post not found or no permission to delete.");
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.ToString());
      return StatusCode(500, "Failed to delete post");
    }
  }
}

