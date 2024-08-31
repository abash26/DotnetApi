using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PostEFController : ControllerBase
{
  IPostRepository _postRepository;

  public PostEFController(IPostRepository postRepository)
  {
    _postRepository = postRepository;
  }

  [HttpGet("GetPosts")]
  public async Task<IEnumerable<Post>> GetPosts()
  {
    return await _postRepository.GetPostsAsync();
  }

  [HttpGet("GetPostsBySearch/{searchParam}")]
  public async Task<IEnumerable<Post>> GetPostsBySearch(string searchParam)
  {
    return await _postRepository.GetPostsBySearchAsync(searchParam);
  }

  [HttpGet("GetSinglePost/{postId}")]
  public async Task<Post?> GetSinglePost(int postId)
  {
    return await _postRepository.GetSinglePostAsync(postId);
  }

  [HttpGet("GetPostsByUser/{userId}")]
  public async Task<IEnumerable<Post>> GetPostsByUser(int userId)
  {
    return await _postRepository.GetPostsByUserAsync(userId);
  }

  [HttpGet("GetMyPosts")]
  public async Task<IEnumerable<Post>> GetMyPosts()
  {
    var userIdValue = User.FindFirst("userId")?.Value;
    int userId;

    if (int.TryParse(userIdValue, out userId))
    {
      return await _postRepository.GetPostsByUserAsync(userId);
    }
    throw new Exception("Failed to get posts");
  }

  [HttpPost("AddPost")]
  public async Task<IActionResult> AddPost(PostDto postDto)
  {
    if (!int.TryParse(User.FindFirst("userId")?.Value, out int userId))
      return BadRequest("Invalid user ID.");

    var post = new Post
    {
      UserId = userId,
      PostTitle = postDto.PostTitle,
      PostContent = postDto.PostContent,
      PostCreated = DateTime.Now,
      PostUpdated = DateTime.Now
    };
    try
    {
      await _postRepository.AddEntityAsync(post);
      return Ok();
    }
    catch (Exception)
    {
      return StatusCode(500, "Failed to add the post due to an internal error.");
    }
  }

  [HttpPut("EditPost")]
  public async Task<IActionResult> EditPost(PostDto postEditDto)
  {
    var post = await _postRepository.GetSinglePostAsync(postEditDto.PostId);

    if (post == null)
      return NotFound($"No post found with ID {postEditDto.PostId}");

    post.PostTitle = postEditDto.PostTitle;
    post.PostContent = postEditDto.PostContent;
    post.PostUpdated = DateTime.UtcNow;

    try
    {
      await _postRepository.UpdatePostAsync(post);
      return Ok();
    }
    catch (Exception)
    {
      return StatusCode(500, "Internal server error while updating the post.");
    }
  }

  [HttpDelete("DeletePost/{postId}")]
  public async Task<IActionResult> DeletePost(int postId)
  {
    var post = await _postRepository.GetSinglePostAsync(postId);

    if (post == null)
      return NotFound($"No post found with ID {postId}");

    await _postRepository.RemoveEntity<Post>(post);

    return Ok($"Post with ID {postId} deleted successfully.");
  }
}

