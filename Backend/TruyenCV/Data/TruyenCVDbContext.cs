using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TruyenCV.Models;

namespace TruyenCV.Data
{
    public partial class TruyenCVDbContext : IdentityDbContext<ApplicationUser>
    {
        public TruyenCVDbContext(DbContextOptions<TruyenCVDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Author> Authors { get; set; } = null!;
        public virtual DbSet<Story> Stories { get; set; } = null!;
        public virtual DbSet<Genre> Genres { get; set; } = null!;
        public virtual DbSet<StoryGenre> StoryGenres { get; set; } = null!;
        public virtual DbSet<Chapter> Chapters { get; set; } = null!;
        public virtual DbSet<Bookmark> Bookmarks { get; set; } = null!;
        public virtual DbSet<Rating> Ratings { get; set; } = null!;
        public virtual DbSet<ReadingHistory> ReadingHistories { get; set; } = null!;
        public virtual DbSet<Comment> Comments { get; set; } = null!;
        public virtual DbSet<FollowStory> FollowStories { get; set; } = null!;
        public virtual DbSet<FollowAuthor> FollowAuthors { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StoryGenre>()
                .HasKey(sg => new { sg.StoryId, sg.GenreId });

            modelBuilder.Entity<FollowStory>()
                .HasKey(fs => new { fs.ApplicationUserId, fs.StoryId });
            modelBuilder.Entity<Bookmark>()
                .HasKey(b => new { b.ApplicationUserId, b.StoryId });
            modelBuilder.Entity<FollowAuthor>()
                .HasKey(fa => new { fa.ApplicationUserId, fa.AuthorId });


        }
    }
}
