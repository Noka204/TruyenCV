using TruyenCV.Dtos.Stories;

namespace TruyenCV.Services;

public interface IStoryService
{
    Task<List<StoryListItemDTO>> GetAllAsync(int? authorId = null, int? primaryGenreId = null, string? q = null);
    Task<StoryDTO?> GetByIdAsync(int id);

    Task<StoryDTO> CreateAsync(StoryCreateDTO dto);
    Task<StoryDTO> UpdateAsync(int id, StoryUpdateDTO dto);
    Task<bool> DeleteAsync(int id);
}
