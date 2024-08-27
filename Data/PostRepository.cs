using DotnetApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetApi.Data;

public class PostRepository : IPostRepository
{
  DataContextEF _entityFramework;
  public PostRepository(IConfiguration config)
  {
    _entityFramework = new DataContextEF(config);
  }

  public async Task<List<Post>> GetPostsAsync()
  {
    return await _entityFramework.Post.ToListAsync();
  }

  public async Task<List<Post>> GetPostsBySearchAsync(string searchParam)
  {
    var lowerCaseSearchParam = searchParam.ToLower();
    return await _entityFramework.Post
        .Where(p => p.PostTitle.ToLower().Contains(lowerCaseSearchParam) || p.PostContent.ToLower().Contains(searchParam))
        .ToListAsync();
  }

  public async Task<Post?> GetSinglePostAsync(int postId)
  {
    return await _entityFramework.Post.FirstOrDefaultAsync(p => p.PostId == postId);
  }

  public async Task<List<Post>> GetPostsByUserAsync(int userId)
  {
    return await _entityFramework.Post.Where(p => p.UserId == userId)
      .ToListAsync();
  }

  public async Task AddEntityAsync<T>(T entityToAdd)
  {
    if (entityToAdd != null)
    {
      await _entityFramework.AddAsync(entityToAdd);
      await _entityFramework.SaveChangesAsync();
    }
  }

  public async Task RemoveEntity<T>(T entityToRemove)
  {
    if (entityToRemove != null)
    {
      _entityFramework.Remove(entityToRemove);
    }
    await _entityFramework.SaveChangesAsync();
  }

  public async Task UpdatePostAsync(Post postToUpdate)
  {
    _entityFramework.Post.Update(postToUpdate);
    await _entityFramework.SaveChangesAsync();
  }
}