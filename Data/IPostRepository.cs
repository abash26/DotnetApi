using DotnetApi.Models;

namespace DotnetApi.Data;

public interface IPostRepository
{
  Task<List<Post>> GetPostsAsync();
  Task<List<Post>> GetPostsBySearchAsync(string searchParam);
  Task<Post?> GetSinglePostAsync(int postId);
  Task<List<Post>> GetPostsByUserAsync(int userId);
  Task AddEntityAsync<T>(T entityToAdd);
  Task UpdatePostAsync(Post postToUpdate);
  Task RemoveEntity<T>(T entityToRemove);
}