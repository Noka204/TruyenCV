using System.ComponentModel.DataAnnotations;

namespace TruyenCV.Dtos.Ratings;

public class RatingCreateDTO
{
    [Required]
    public int StoryId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "?i?m ?ánh giá ph?i t? 1 ??n 5.")]
    public byte Score { get; set; }
}
