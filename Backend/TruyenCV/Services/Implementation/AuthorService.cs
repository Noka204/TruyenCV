using TruyenCV.Data.Repositories.Interface;
using TruyenCV.Dtos.Authors;
using TruyenCV.Dtos.Stories;
using TruyenCV.Models;

namespace TruyenCV.Services.Implementation;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _repo;
    public AuthorService(IAuthorRepository repo) => _repo = repo;

    public async Task<List<AuthorListItemDTO>> GetAllAsync()
    {
        var authors = await _repo.GetAllAsync();
        return authors.Select(a => new AuthorListItemDTO
        {
            AuthorId = a.AuthorId,
            DisplayName = a.DisplayName,
            AvatarUrl = a.AvatarUrl,
            CreatedAt = a.CreatedAt
        }).ToList();
    }

    public async Task<List<AuthorListItemDTO>> GetApprovedAsync()
    {
        var authors = await _repo.GetApprovedAsync();
        return authors.Select(a => new AuthorListItemDTO
        {
            AuthorId = a.AuthorId,
            DisplayName = a.DisplayName,
            AvatarUrl = a.AvatarUrl,
            CreatedAt = a.CreatedAt
        }).ToList();
    }

    public async Task<AuthorDTO?> GetByIdAsync(int id)
    {
        var a = await _repo.GetByIdAsync(id);
        if (a is null) return null;

        return new AuthorDTO
        {
            AuthorId = a.AuthorId,
            DisplayName = a.DisplayName,
            Bio = a.Bio,
            AvatarUrl = a.AvatarUrl,
            ApplicationUserId = a.ApplicationUserId,
            CreatedAt = a.CreatedAt
        };
    }

    public async Task<int> CreateAsync(AuthorCreateDTO dto)
    {
        var author = BuildAndValidateAuthor(dto);

        author.CreatedAt = DateTime.UtcNow;

        var newId = await _repo.CreateAsync(author);
        return newId;
    }

    public async Task<bool> UpdateAsync(int id, AuthorUpdateDTO dto)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null) return false;

        var author = BuildAndValidateAuthor(dto);
        author.AuthorId = id;
        author.CreatedAt = existing.CreatedAt; // giữ CreatedAt cũ

        return await _repo.UpdateAsync(author);
    }

    public async Task<bool> DeleteAsync(int id)
        => await _repo.DeleteAsync(id);

    public async Task<List<StoryBriefDTO>?> GetStoriesAsync(int authorId)
    {
        var exists = await _repo.ExistsAsync(authorId);
        if (!exists) return null;

        var rows = await _repo.GetStoriesByAuthorAsync(authorId);
        return rows.Select(x => new StoryBriefDTO
        {
            StoryId = x.StoryId,
            Title = x.Title,
            Status = x.Status,
            UpdatedAt = x.UpdatedAt
        }).ToList();
    }

    // ----- helpers -----

    private static Author BuildAndValidateAuthor(dynamic dto)
    {
        var displayName = (dto.DisplayName as string)?.Trim();
        var bio = string.IsNullOrWhiteSpace(dto.Bio as string) ? null : ((string)dto.Bio).Trim();
        var avatarUrl = string.IsNullOrWhiteSpace(dto.AvatarUrl as string) ? null : ((string)dto.AvatarUrl).Trim();
        var applicationUserId = string.IsNullOrWhiteSpace(dto.ApplicationUserId as string) ? null : ((string)dto.ApplicationUserId).Trim();

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Tên hiển thị không được để trống.");

        if (displayName.Length > 150)
            throw new ArgumentException("Tên hiển thị tối đa 150 ký tự.");

        if (avatarUrl is not null && avatarUrl.Length > 500)
            throw new ArgumentException("Đường dẫn avatar tối đa 500 ký tự.");

        if (applicationUserId is not null && applicationUserId.Length > 450)
            throw new ArgumentException("ApplicationUserId tối đa 450 ký tự.");

        return new Author
        {
            DisplayName = displayName,
            Bio = bio,
            AvatarUrl = avatarUrl,
            ApplicationUserId = applicationUserId
        };
    }

    // ----- Author Approval Workflow -----

    public async Task<int> SubmitAuthorRequestAsync(string userId, AuthorRequestDTO dto)
    {
        var existing = await _repo.GetByApplicationUserIdAsync(userId);
        if (existing is not null)
            throw new ArgumentException("Bạn đã gửi yêu cầu đăng ký làm tác giả rồi.");

        var displayName = dto.DisplayName?.Trim();
        var bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim();
        var avatarUrl = string.IsNullOrWhiteSpace(dto.AvatarUrl) ? null : dto.AvatarUrl.Trim();

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Tên hiển thị không được để trống.");

        if (displayName.Length > 150)
            throw new ArgumentException("Tên hiển thị tối đa 150 ký tự.");

        if (avatarUrl is not null && avatarUrl.Length > 500)
            throw new ArgumentException("Đường dẫn avatar tối đa 500 ký tự.");

        var author = new Author
        {
            DisplayName = displayName,
            Bio = bio,
            AvatarUrl = avatarUrl,
            ApplicationUserId = userId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        var newId = await _repo.CreateAsync(author);
        return newId;
    }

    public async Task<AuthorStatusDTO?> GetMyAuthorStatusAsync(string userId)
    {
        var author = await _repo.GetByApplicationUserIdAsync(userId);
        if (author is null) return null;

        return new AuthorStatusDTO
        {
            AuthorId = author.AuthorId,
            DisplayName = author.DisplayName,
            Bio = author.Bio,
            AvatarUrl = author.AvatarUrl,
            Status = author.Status,
            CreatedAt = author.CreatedAt,
            ApprovedAt = author.ApprovedAt
        };
    }

    public async Task<List<AuthorPendingListDTO>> GetPendingAuthorsAsync()
    {
        var authors = await _repo.GetPendingAuthorsAsync();
        return authors.Select(a => new AuthorPendingListDTO
        {
            AuthorId = a.AuthorId,
            DisplayName = a.DisplayName,
            Bio = a.Bio,
            AvatarUrl = a.AvatarUrl,
            UserEmail = a.ApplicationUser?.Email ?? "",
            UserFullName = a.ApplicationUser?.FullName ?? "",
            CreatedAt = a.CreatedAt
        }).ToList();
    }

    public async Task<bool> ApproveAuthorAsync(int authorId, string adminId)
    {
        var author = await _repo.GetByIdAsync(authorId);
        if (author is null) return false;

        if (author.Status == "Approved")
            throw new ArgumentException("Tác giả này đã được duyệt rồi.");

        author.Status = "Approved";
        author.ApprovedAt = DateTime.UtcNow;
        author.ApprovedBy = adminId;

        return await _repo.UpdateAsync(author);
    }

    public async Task<bool> RejectAuthorAsync(int authorId)
    {
        return await _repo.DeleteAsync(authorId);
    }
}
