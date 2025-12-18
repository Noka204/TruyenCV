using TruyenCV.Dtos.Stories;
using TruyenCV.Models;
using TruyenCV.Repositories;

namespace TruyenCV.Services;

public class StoryService : IStoryService
{
    private readonly IStoryRepository _repo;
    public StoryService(IStoryRepository repo) => _repo = repo;

    public async Task<List<StoryListItemDTO>> GetAllAsync(int? authorId = null, int? primaryGenreId = null, string? q = null)
    {
        var list = await _repo.GetAllAsync(authorId, primaryGenreId, q);
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
            PrimaryGenreId = n.PrimaryGenreId,
            Status = n.Status,
            CreatedAt = now,
            UpdatedAt = now
        };

        var id = await _repo.CreateAsync(entity);
        var created = await _repo.GetByIdAsync(id);

        // an toàn: nếu repo tạo xong mà không lấy được thì coi như lỗi hệ thống
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
            PrimaryGenreId = n.PrimaryGenreId,
            Status = n.Status,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        var ok = await _repo.UpdateAsync(entity);
        if (!ok) throw new KeyNotFoundException("Không tìm thấy truyện để cập nhật.");

        var updated = await _repo.GetByIdAsync(id);
        if (updated is null) throw new InvalidOperationException("Cập nhật truyện thất bại. Vui lòng thử lại.");

        return MapToDTO(updated);
    }

    public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

    // ---------- helpers ----------

    private static NormalizedStory Normalize(StoryCreateDTO dto) => new(
        Title: (dto.Title ?? "").Trim(),
        AuthorId: dto.AuthorId,
        Description: string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
        CoverImage: string.IsNullOrWhiteSpace(dto.CoverImage) ? null : dto.CoverImage.Trim(),
        PrimaryGenreId: dto.PrimaryGenreId,
        Status: (dto.Status ?? "").Trim()
    );

    private static NormalizedStory Normalize(StoryUpdateDTO dto) => new(
        Title: (dto.Title ?? "").Trim(),
        AuthorId: dto.AuthorId,
        Description: string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
        CoverImage: string.IsNullOrWhiteSpace(dto.CoverImage) ? null : dto.CoverImage.Trim(),
        PrimaryGenreId: dto.PrimaryGenreId,
        Status: (dto.Status ?? "").Trim()
    );

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

        if (n.CoverImage is not null && n.CoverImage.Length > 500)
            throw new ArgumentException("Đường dẫn ảnh bìa tối đa 500 ký tự.");
    }

    private readonly record struct NormalizedStory(
        string Title,
        int AuthorId,
        string? Description,
        string? CoverImage,
        int? PrimaryGenreId,
        string Status
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
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
