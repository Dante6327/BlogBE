namespace Blog.Model;

/// <summary>
/// 게시글 수정 히스토리를 추적하기 위한 엔티티
/// </summary>
public class PostRevision
{
    public long Id { get; set; }
    public long PostId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public int RevisionNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 네비게이션 속성
    public Post Post { get; set; } = null!;
}