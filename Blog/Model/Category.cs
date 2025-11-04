namespace Blog.Model;

public class Category
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 네비게이션 속성
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}