using System.ComponentModel.DataAnnotations;

namespace TruyenCV.Dtos.Stories;

public class StoryCreateDTO
{
    [Required, StringLength(200)]
    public string Title { get; set; } = null!;

    [Required]
    public int AuthorId { get; set; }

    public string? Description { get; set; }

    [StringLength(500)]
    public string? CoverImage { get; set; }

    public int? PrimaryGenreId { get; set; }

    // 2 trạng thái tiếng Việt
    [Required, StringLength(20)]
    public string Status { get; set; } = "Đang tiến hành";
}
