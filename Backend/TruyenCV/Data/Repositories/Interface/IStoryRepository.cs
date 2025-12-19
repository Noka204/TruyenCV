using TruyenCV.Models;

namespace TruyenCV.Data.Repositories.Interface;

public interface IStoryRepository
{
    Task<List<Story>> GetAllAsync(int? authorId = null, int? genreId = null, string? q = null);
    Task<List<Story>> GetByGenresAsync(IReadOnlyCollection<int> genreIds);
    Task<Story?> GetByIdAsync(int id);

    Task<bool> AuthorExistsAsync(int authorId);
    Task<bool> GenreExistsAsync(int genreId);
    Task<bool> GenresExistAsync(IReadOnlyCollection<int> genreIds);

    Task<int> CreateAsync(Story story, IReadOnlyCollection<int> genreIds);
    Task<bool> UpdateAsync(Story story, IReadOnlyCollection<int> genreIds);

    Task<bool> DeleteAsync(int id);
}
