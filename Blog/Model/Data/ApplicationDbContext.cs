using Blog.Model;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PostTag> PostTags { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<PostRevision> PostRevisions { get; set; }
    public DbSet<MediaFile> MediaFiles { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User 설정
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(u => u.Username).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Bio).HasColumnType("text");
            entity.Property(u => u.AvatarUrl).HasMaxLength(500);
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

            // 인덱스
            entity.HasIndex(u => u.Email).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            entity.HasIndex(u => u.Username).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            entity.HasIndex(u => u.Role);

            // Soft Delete 필터
            entity.HasQueryFilter(u => u.DeletedAt == null);
        });

        // Post 설정
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Title).HasMaxLength(255).IsRequired();
            entity.Property(p => p.Slug).HasMaxLength(255).IsRequired();
            entity.Property(p => p.Content).HasColumnType("text").IsRequired();
            entity.Property(p => p.Summary).HasMaxLength(500);
            entity.Property(p => p.ThumbnailUrl).HasMaxLength(500);
            entity.Property(p => p.SeoKeywords).HasMaxLength(500);
            entity.Property(p => p.MetaDescription).HasMaxLength(160);
            entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

            // 인덱스
            entity.HasIndex(p => p.Slug).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            entity.HasIndex(p => p.UserId);
            entity.HasIndex(p => p.CategoryId);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.CreatedAt);
            entity.HasIndex(p => p.PublishedAt);
            entity.HasIndex(p => new { p.Status, p.CreatedAt });

            // Soft Delete 필터
            entity.HasQueryFilter(p => p.DeletedAt == null);

            // 관계
            entity.HasOne(p => p.User)
                  .WithMany(u => u.Posts)
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Posts)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Category 설정
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Slug).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Description).HasColumnType("text");

            // 인덱스
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.HasIndex(c => c.DisplayOrder);
        });

        // Tag 설정
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
            entity.Property(t => t.Slug).HasMaxLength(100).IsRequired();

            // 인덱스
            entity.HasIndex(t => t.Slug).IsUnique();
            entity.HasIndex(t => t.Name);
        });

        // PostTag 다대다 관계 설정
        modelBuilder.Entity<PostTag>(entity =>
        {
            entity.HasKey(pt => new { pt.PostId, pt.TagId });

            entity.HasOne(pt => pt.Post)
                  .WithMany(p => p.PostTags)
                  .HasForeignKey(pt => pt.PostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pt => pt.Tag)
                  .WithMany(t => t.PostTags)
                  .HasForeignKey(pt => pt.TagId)
                  .OnDelete(DeleteBehavior.Cascade);

            // 인덱스
            entity.HasIndex(pt => pt.PostId);
            entity.HasIndex(pt => pt.TagId);
        });

        // Comment 설정 (계층 구조 - 대댓글)
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Content).HasColumnType("text").IsRequired();

            // 인덱스
            entity.HasIndex(c => c.PostId);
            entity.HasIndex(c => c.UserId);
            entity.HasIndex(c => c.ParentCommentId);
            entity.HasIndex(c => c.CreatedAt);

            // Soft Delete 필터
            entity.HasQueryFilter(c => c.DeletedAt == null);

            // 관계
            entity.HasOne(c => c.Post)
                  .WithMany(p => p.Comments)
                  .HasForeignKey(c => c.PostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.User)
                  .WithMany(u => u.Comments)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // 대댓글 관계 (Self-referencing)
            entity.HasOne(c => c.ParentComment)
                  .WithMany(c => c.Replies)
                  .HasForeignKey(c => c.ParentCommentId)
                  .OnDelete(DeleteBehavior.Restrict);  // 부모 댓글 삭제 시 자식은 유지
        });

        // PostRevision 설정
        modelBuilder.Entity<PostRevision>(entity =>
        {
            entity.HasKey(pr => pr.Id);
            entity.Property(pr => pr.Title).HasMaxLength(255).IsRequired();
            entity.Property(pr => pr.Content).HasColumnType("text").IsRequired();
            entity.Property(pr => pr.Summary).HasMaxLength(500);

            // 인덱스
            entity.HasIndex(pr => pr.PostId);
            entity.HasIndex(pr => pr.CreatedAt);

            // 관계
            entity.HasOne(pr => pr.Post)
                  .WithMany(p => p.Revisions)
                  .HasForeignKey(pr => pr.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // MediaFile 설정
        modelBuilder.Entity<MediaFile>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.FileName).HasMaxLength(255).IsRequired();
            entity.Property(m => m.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(m => m.CdnUrl).HasMaxLength(500);
            entity.Property(m => m.MimeType).HasMaxLength(100).IsRequired();
            entity.Property(m => m.AltText).HasMaxLength(255);

            // 인덱스
            entity.HasIndex(m => m.UserId);
            entity.HasIndex(m => m.CreatedAt);

            // Soft Delete 필터
            entity.HasQueryFilter(m => m.DeletedAt == null);

            // 관계
            entity.HasOne(m => m.User)
                  .WithMany()
                  .HasForeignKey(m => m.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // UserSession 설정
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.RefreshToken).HasMaxLength(500).IsRequired();
            entity.Property(s => s.DeviceInfo).HasMaxLength(500);
            entity.Property(s => s.IpAddress).HasMaxLength(45);

            // 인덱스
            entity.HasIndex(s => s.RefreshToken).IsUnique();
            entity.HasIndex(s => s.UserId);
            entity.HasIndex(s => s.ExpiresAt);

            // 관계
            entity.HasOne(s => s.User)
                  .WithMany(u => u.Sessions)
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Bookmark 설정 (다대다)
        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(b => new { b.UserId, b.PostId });

            entity.HasIndex(b => b.UserId);
            entity.HasIndex(b => b.PostId);
            entity.HasIndex(b => b.CreatedAt);

            // 관계
            entity.HasOne(b => b.User)
                  .WithMany(u => u.Bookmarks)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.Post)
                  .WithMany()
                  .HasForeignKey(b => b.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}