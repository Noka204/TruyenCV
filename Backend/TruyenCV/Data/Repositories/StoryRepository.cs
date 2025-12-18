using Microsoft.EntityFrameworkCore;
using TruyenCV.Data;
using TruyenCV.Models;

namespace TruyenCV.Repositories;

public class StoryRepository : IStoryRepository
{
    private readonly TruyenCVDbContext _db;
    public StoryRepository(TruyenCVDbContext db) => _db = db;

    public async Task<List<Story>> GetAllAsync(int? authorId = null, int? primaryGenreId = null, string? q = null)
    {
        IQueryable<Story> query = _db.Stories.AsNoTracking();

        if (authorId is not null)
            query = query.Where(s => s.AuthorId == authorId.Value);

        if (primaryGenreId is not null)
            query = query.Where(s => s.PrimaryGenreId == primaryGenreId.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(s => s.Title.Contains(q));
        }

        return await query
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();
    }

    public Task<Story?> GetByIdAsync(int id)
        => _db.Stories.AsNoTracking().FirstOrDefaultAsync(s => s.StoryId == id);

    public Task<bool> ExistsAsync(int id)
        => _db.Stories.AnyAsync(s => s.StoryId == id);

    public Task<bool> AuthorExistsAsync(int authorId)
        => _db.Authors.AnyAsync(a => a.AuthorId == authorId);

    public Task<bool> GenreExistsAsync(int genreId)
        => _db.Genres.AnyAsync(g => g.GenreId == genreId);

    public async Task<int> CreateAsync(Story story)
    {
        _db.Stories.Add(story);
        await _db.SaveChangesAsync();
        return story.StoryId;
    }

    public async Task<bool> UpdateAsync(Story story)
    {
        var exists = await _db.Stories.AnyAsync(s => s.StoryId == story.StoryId);
        if (!exists) return false;

        _db.Stories.Update(story);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Stories.FirstOrDefaultAsync(s => s.StoryId == id);
        if (entity is null) return false;

        _db.Stories.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}
