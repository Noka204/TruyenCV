using TruyenCV.Data.Repositories.Interface;
using TruyenCV.Dtos.Stories;
using TruyenCV.Models;
using TruyenCV.Services.IService;

namespace TruyenCV.Services.Implementation;

public class StoryService : IStoryService
{
    private readonly IStoryRepository _repo;
    public StoryService(IStoryRepository repo) => _repo = repo;

    public async Task<List<StoryListItemDTO>> GetAllAsync(string? q = null)
    {
        var list = await _repo.GetAllAsync(authorId: null, genreId: null, q);
        return list.Select(MapToListItemDTO).ToList();
    }

    public async Task<List<StoryListItemDTO>> GetByAuthorAsync(int authorId)
    {
        if (!await _repo.AuthorExistsAsync(authorId))
            throw new KeyNotFoundException("Không tìm thấy tác giả.");

        var list = await _repo.GetAllAsync(authorId, genreId: null, q: null);
        return list.Select(MapToListItemDTO).ToList();
    }

    public async Task<List<StoryListItemDTO>> GetByGenreAsync(int genreId)
    {
        if (!await _repo.GenreExistsAsync(genreId))
            throw new KeyNotFoundException("Không tìm thấy thể loại.");

        var list = await _repo.GetAllAsync(authorId: null, genreId, q: null);
        return list.Select(MapToListItemDTO).ToList();
    }

    public async Task<List<StoryListItemDTO>> GetByGenresAsync(List<int>? genreIds)
    {
        if (genreIds is null || genreIds.Count == 0)
            throw new ArgumentException("Danh sách thể loại không được để trống.");

        var distinctIds = genreIds.Where(x => x > 0).Distinct().ToList();
        if (distinctIds.Count == 0)
            throw new ArgumentException("Danh sách thể loại không hợp lệ.");

        if (!await _repo.GenresExistAsync(distinctIds))
            throw new KeyNotFoundException("Có thể loại không tồn tại.");

        var list = await _repo.GetByGenresAsync(distinctIds);
        return list.Select(MapToListItemDTO).ToList();
    }

    public async Task<StoryDTO?> GetByIdAsync(int id)
    {
        var s = await _repo.GetByIdAsync(id);
        return s is null ? null : MapToDTO(s);
    }

    public async Task<StoryDTO> CreateAsync(StoryCreateDTO dto)
    {
        var n = Normalize(dto);
        await ValidateAsync(n);

        var now = DateTime.UtcNow;

        var entity = new Story
        {
            Title = n.Title,
            AuthorId = n.AuthorId,
            Description = n.Description,
            CoverImage = n.CoverImage,
            BannerImage = n.BannerImage,           // ✅ banner
            PrimaryGenreId = n.PrimaryGenreId,
            Status = n.Status,                     // ✅ tiếng Việt
            CreatedAt = now,
            UpdatedAt = now
        };

        var id = await _repo.CreateAsync(entity, n.GenreIds);

        var created = await _repo.GetByIdAsync(id);
        if (created is null) throw new InvalidOperationException("Tạo truyện thất bại. Vui lòng thử lại.");

        return MapToDTO(created);
    }

    public async Task<StoryDTO> UpdateAsync(int id, StoryUpdateDTO dto)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null) throw new KeyNotFoundException("Không tìm thấy truyện để cập nhật.");

        var n = Normalize(dto);
        await ValidateAsync(n);

        var entity = new Story
        {
            StoryId = id,
            Title = n.Title,
            AuthorId = n.AuthorId,
            Description = n.Description,
            CoverImage = n.CoverImage,
            BannerImage = n.BannerImage,          // ✅ banner
            PrimaryGenreId = n.PrimaryGenreId,
            Status = n.Status,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        var ok = await _repo.UpdateAsync(entity, n.GenreIds);
        if (!ok) throw new KeyNotFoundException("Không tìm thấy truyện để cập nhật.");

        var updated = await _repo.GetByIdAsync(id);
        if (updated is null) throw new InvalidOperationException("Cập nhật truyện thất bại. Vui lòng thử lại.");

        return MapToDTO(updated);
    }

    public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

    // ---------- helpers ----------

    private static NormalizedStory Normalize(StoryCreateDTO dto)
        => NormalizeCore(
            dto.Title, dto.AuthorId, dto.Description,
            dto.CoverImage, dto.BannerImage,
            dto.PrimaryGenreId, dto.Status, dto.GenreIds
        );

    private static NormalizedStory Normalize(StoryUpdateDTO dto)
        => NormalizeCore(
            dto.Title, dto.AuthorId, dto.Description,
            dto.CoverImage, dto.BannerImage,
            dto.PrimaryGenreId, dto.Status, dto.GenreIds
        );

    private static NormalizedStory NormalizeCore(
        string? title, int authorId, string? description,
        string? coverImage, string? bannerImage,
        int? primaryGenreId, string? status, List<int>? genreIds
    )
    {
        var st = string.IsNullOrWhiteSpace(status) ? "Đang tiến hành" : status.Trim();

        var ids = (genreIds ?? new List<int>())
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        // ✅ rule mặc định: PrimaryGenreId luôn nằm trong StoryGenres để đồng bộ
        if (primaryGenreId is not null)
            ids = ids.Append(primaryGenreId.Value).Distinct().ToList();

        return new NormalizedStory(
            Title: (title ?? "").Trim(),
            AuthorId: authorId,
            Description: string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CoverImage: string.IsNullOrWhiteSpace(coverImage) ? null : coverImage.Trim(),
            BannerImage: string.IsNullOrWhiteSpace(bannerImage) ? null : bannerImage.Trim(),
            PrimaryGenreId: primaryGenreId,
            Status: st,
            GenreIds: ids
        );
    }

    private async Task ValidateAsync(NormalizedStory n)
    {
        if (string.IsNullOrWhiteSpace(n.Title))
            throw new ArgumentException("Tiêu đề truyện không được để trống.");

        if (n.Title.Length > 200)
            throw new ArgumentException("Tiêu đề truyện tối đa 200 ký tự.");

        if (n.Status is not ("Đang tiến hành" or "Đã hoàn thành"))
            throw new ArgumentException("Trạng thái không hợp lệ. Chỉ chấp nhận: 'Đang tiến hành' hoặc 'Đã hoàn thành'.");

        if (!await _repo.AuthorExistsAsync(n.AuthorId))
            throw new ArgumentException("Tác giả không tồn tại.");

        if (n.PrimaryGenreId is not null && !await _repo.GenreExistsAsync(n.PrimaryGenreId.Value))
            throw new ArgumentException("Thể loại chính không tồn tại.");

        if (n.GenreIds.Count > 0 && !await _repo.GenresExistAsync(n.GenreIds))
            throw new ArgumentException("Có thể loại không tồn tại.");

        if (n.CoverImage is not null && n.CoverImage.Length > 500)
            throw new ArgumentException("Đường dẫn ảnh bìa tối đa 500 ký tự.");

        if (n.BannerImage is not null && n.BannerImage.Length > 500)
            throw new ArgumentException("Đường dẫn banner tối đa 500 ký tự.");
    }

    private readonly record struct NormalizedStory(
        string Title,
        int AuthorId,
        string? Description,
        string? CoverImage,
        string? BannerImage,
        int? PrimaryGenreId,
        string Status,
        List<int> GenreIds
    );

    private static StoryListItemDTO MapToListItemDTO(Story s) => new()
    {
        StoryId = s.StoryId,
        Title = s.Title,
        AuthorId = s.AuthorId,
        PrimaryGenreId = s.PrimaryGenreId,
        Status = s.Status,
        UpdatedAt = s.UpdatedAt
    };

    private static StoryDTO MapToDTO(Story s) => new()
    {
        StoryId = s.StoryId,
        Title = s.Title,
        AuthorId = s.AuthorId,
        PrimaryGenreId = s.PrimaryGenreId,
        Status = s.Status,
        Description = s.Description,
        CoverImage = s.CoverImage,
        BannerImage = s.BannerImage,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt,
        GenreIds = s.StoryGenres.Select(x => x.GenreId).Distinct().ToList(),
        Chapters = s.Chapters
            .OrderBy(c => c.ChapterNumber)
            .Select(c => new
            {
                c.ChapterId,
                c.ChapterNumber,
                c.Title,
                c.ReadCont,
                c.CreatedAt,
                c.UpdatedAt
            })
            .Cast<object>()
            .ToList()
    };
}
