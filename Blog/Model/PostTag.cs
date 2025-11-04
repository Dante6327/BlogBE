namespace Blog.Model;

/// <summary>
/// Posts와 Tags의 다대다 관계를 위한 중간 테이블
/// </summary>
public class PostTag
{
    public long PostId { get; set; }
    public long TagId { get; set; }

    // 네비게이션 속성
    public Post Post { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}