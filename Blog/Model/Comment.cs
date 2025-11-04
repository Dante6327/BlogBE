namespace Blog.Model;

public class Comment
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public long UserId { get; set; }
    public long? ParentCommentId { get; set; }  // 대댓글을 위한 부모 댓글 ID
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    // 네비게이션 속성
    public Post Post { get; set; } = null!;
    public User User { get; set; } = null!;
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}