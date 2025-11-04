namespace Blog.Model;

/// <summary>
/// JWT 리프레시 토큰을 관리하기 위한 엔티티
/// </summary>
public class UserSession
{
    public long Id { get; set; }
    public long UserId { get; set; }

    public string RefreshToken { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }

    // 네비게이션 속성
    public User User { get; set; } = null!;
}