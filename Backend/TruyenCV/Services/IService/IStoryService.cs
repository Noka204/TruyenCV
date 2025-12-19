using TruyenCV.Dtos.Stories;

namespace TruyenCV.Services.IService;

public interface IStoryService
{
    Task<List<StoryListItemDTO>> GetAllAsync(string? q = null);
    Task<List<StoryListItemDTO>> GetByAuthorAsync(int authorId);
    Task<List<StoryListItemDTO>> GetByGenreAsync(int genreId);
    Task<List<StoryListItemDTO>> GetByGenresAsync(List<int>? genreIds);
    Task<StoryDTO?> GetByIdAsync(int id);

    Task<StoryDTO> CreateAsync(StoryCreateDTO dto);
    Task<StoryDTO> UpdateAsync(int id, StoryUpdateDTO dto);
    Task<bool> DeleteAsync(int id);
}
