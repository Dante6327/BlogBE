namespace Blog.DTOs.Responses;

/// <summary>
/// 게시글 목록 조회 시 반환하는 DTO
/// PostResponse보다 간단 (Content는 제외, Summary만)
/// </summary>
public class PostListResponse
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int ReadingTimeMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public bool IsFeatured { get; set; }

    public AuthorDto Author { get; set; } = null!;
    public CategoryDto? Category { get; set; }
    public List<TagDto> Tags { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}