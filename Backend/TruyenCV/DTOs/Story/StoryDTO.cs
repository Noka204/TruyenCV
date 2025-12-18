namespace TruyenCV.Dtos.Stories;

public class StoryDTO : StoryListItemDTO
{
    public string? Description { get; set; }
    public string? CoverImage { get; set; }
    public DateTime CreatedAt { get; set; }
}
