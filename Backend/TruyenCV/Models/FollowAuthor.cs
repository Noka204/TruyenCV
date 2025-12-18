using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TruyenCV.Models;

[Table("FollowAuthors", Schema = "dbo")]
public class FollowAuthor
{
    [Key]
    [Column("follow_author_id")]
    public int FollowAuthorId { get; set; }
    // follower (user login)
    [Required, StringLength(450)]
    [Column("user_id")]
    public string ApplicationUserId { get; set; } = null!;

    [ForeignKey(nameof(ApplicationUserId))]
    public ApplicationUser Follower { get; set; } = null!;

    // followed author (Authors.author_id)
    [Column("author_id")]
    public int AuthorId { get; set; }

    [ForeignKey(nameof(AuthorId))]
    public Author Author { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
