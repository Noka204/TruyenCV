namespace TruyenCV.Dtos.Stories;

public class StoryCreateDTO
{
    public string? Title { get; set; }
    public int AuthorId { get; set; }
    public string? Description { get; set; }

    public string? CoverImage { get; set; }
    public string? BannerImage { get; set; }     // ✅ banner

    public int? PrimaryGenreId { get; set; }
    public string? Status { get; set; }          // "Đang tiến hành" | "Đã hoàn thành"

    public List<int>? GenreIds { get; set; }     // ✅ list thể loại cho StoryGenres
}
