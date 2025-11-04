namespace Blog.Model;

/// <summary>
/// 업로드된 이미지 및 파일의 메타데이터를 관리
/// </summary>
public class MediaFile
{
    public long Id { get; set; }
    public long UserId { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? CdnUrl { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    // 이미지인 경우
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? AltText { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    // 네비게이션 속성
    public User User { get; set; } = null!;
}