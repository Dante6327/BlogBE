namespace Blog.Model;

public class User
{
    // Primary Key
    public long Id { get; set; }

    // 인증 정보
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    // 프로필 정보
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }

    // 권한
    public UserRole Role { get; set; } = UserRole.User;

    // 타임스탬프
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    // 네비게이션 속성
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}

public enum UserRole
{
    User = 0,
    Editor = 1,
    Admin = 2
}