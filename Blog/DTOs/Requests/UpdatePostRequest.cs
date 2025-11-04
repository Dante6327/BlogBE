using System.ComponentModel.DataAnnotations;

namespace Blog.DTOs.Requests;

/// <summary>
/// 게시글 수정 요청 DTO
/// PUT /api/posts/{id} 호출 시 사용
/// CreatePostRequest와 거의 동일하지만, 분리하는 것이 좋은 설계
/// (나중에 수정 가능한 필드와 생성 시 필드가 다를 수 있음)
/// </summary>
public class UpdatePostRequest
{
    [Required(ErrorMessage = "제목은 필수입니다.")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "제목은 1~255자여야 합니다.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "본문은 필수입니다.")]
    public string Content { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "요약은 최대 500자입니다.")]
    public string? Summary { get; set; }

    public long? CategoryId { get; set; }

    public List<long> TagIds { get; set; } = new();

    [Url(ErrorMessage = "올바른 URL 형식이 아닙니다.")]
    public string? ThumbnailUrl { get; set; }

    [StringLength(500, ErrorMessage = "SEO 키워드는 최대 500자입니다.")]
    public string? SeoKeywords { get; set; }

    [StringLength(160, ErrorMessage = "메타 설명은 최대 160자입니다.")]
    public string? MetaDescription { get; set; }

    public string Status { get; set; } = "Draft";

    public bool IsFeatured { get; set; }
}