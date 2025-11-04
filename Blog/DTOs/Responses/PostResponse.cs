namespace Blog.DTOs.Responses;

/// <summary>
/// 게시글 조회 시 반환하는 DTO
/// Entity(Post.cs)의 모든 필드를 보여주지 않고, 필요한 것만 선택
/// </summary>
public class PostResponse
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int ReadingTimeMinutes { get; set; }
    public string Status { get; set; } = string.Empty;  // "Draft", "Published" 등
    public int ViewCount { get; set; }
    public bool IsFeatured { get; set; }

    // 작성자 정보 (JOIN된 데이터)
    public AuthorDto Author { get; set; } = null!;

    // 카테고리 정보
    public CategoryDto? Category { get; set; }

    // 태그 목록
    public List<TagDto> Tags { get; set; } = new();

    // 타임스탬프
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

/// <summary>
/// 작성자 정보 (민감 정보 제외)
/// User 테이블에는 PasswordHash 등이 있지만, 여기서는 공개 정보만
/// </summary>
public class AuthorDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

/// <summary>
/// 카테고리 정보
/// </summary>
public class CategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

/// <summary>
/// 태그 정보
/// </summary>
public class TagDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}