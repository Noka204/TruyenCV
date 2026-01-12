using TruyenCV.Dtos.Authors;
using TruyenCV.Dtos.Stories;

public interface IAuthorService
{
    Task<List<AuthorListItemDTO>> GetAllAsync();
    Task<List<AuthorListItemDTO>> GetApprovedAsync();
    Task<AuthorDTO?> GetByIdAsync(int id);

    Task<int> CreateAsync(AuthorCreateDTO dto);          // trả newId
    Task<bool> UpdateAsync(int id, AuthorUpdateDTO dto); // true/false (not found)
    Task<bool> DeleteAsync(int id);                      // true/false (not found)

    Task<List<StoryBriefDTO>?> GetStoriesAsync(int authorId); // null = không có author

    // Author approval workflow
    Task<int> SubmitAuthorRequestAsync(string userId, AuthorRequestDTO dto);
    Task<AuthorStatusDTO?> GetMyAuthorStatusAsync(string userId);
    Task<List<AuthorPendingListDTO>> GetPendingAuthorsAsync();
    Task<bool> ApproveAuthorAsync(int authorId, string adminId);
    Task<bool> RejectAuthorAsync(int authorId);
}
