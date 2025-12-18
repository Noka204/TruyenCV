using TruyenCV.Models;

namespace TruyenCV.Repositories;

public interface IStoryRepository
{
    Task<List<Story>> GetAllAsync(int? authorId = null, int? primaryGenreId = null, string? q = null);
    Task<Story?> GetByIdAsync(int id);

    Task<bool> ExistsAsync(int id);
    Task<bool> AuthorExistsAsync(int authorId);
    Task<bool> GenreExistsAsync(int genreId);

    Task<int> CreateAsync(Story story);
    Task<bool> UpdateAsync(Story story);
    Task<bool> DeleteAsync(int id);
}
