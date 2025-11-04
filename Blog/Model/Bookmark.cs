namespace Blog.Model;

/// <summary>
/// 사용자의 게시글 북마크 (다대다 관계)
/// </summary>
public class Bookmark
{
    public long UserId { get; set; }
    public long PostId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 네비게이션 속성
    public User User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}