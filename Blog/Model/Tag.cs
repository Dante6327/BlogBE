namespace Blog.Model;

public class Tag
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 네비게이션 속성
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}