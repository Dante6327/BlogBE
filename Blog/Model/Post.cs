namespace Blog.Model;

public class Post
{
    // Primary Key
    public long Id { get; set; }

    // Foreign Keys
    public long UserId { get; set; }
    public long? CategoryId { get; set; }

    // 콘텐츠
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }

    // SEO 및 메타데이터
    public string? ThumbnailUrl { get; set; }
    public int ReadingTimeMinutes { get; set; }
    public string? SeoKeywords { get; set; }
    public string? MetaDescription { get; set; }

    // 상태 관리
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public int ViewCount { get; set; }
    public bool IsFeatured { get; set; }

    // 타임스탬프
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // 네비게이션 속성
    public User User { get; set; } = null!;
    public Category? Category { get; set; }
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostRevision> Revisions { get; set; } = new List<PostRevision>();
}

public enum PostStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2,
    Scheduled = 3
}