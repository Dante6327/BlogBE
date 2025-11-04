using System.ComponentModel.DataAnnotations;

namespace Blog.DTOs.Requests;

/// <summary>
/// 게시글 생성 요청 DTO
/// 클라이언트가 POST /api/posts 호출 시 보내는 데이터
/// </summary>
public class CreatePostRequest
{
    /// <summary>
    /// 게시글 제목 (필수, 1~255자)
    /// </summary>
    [Required(ErrorMessage = "제목은 필수입니다.")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "제목은 1~255자여야 합니다.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 게시글 본문 (필수)
    /// </summary>
    [Required(ErrorMessage = "본문은 필수입니다.")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 요약 (선택, 최대 500자)
    /// </summary>
    [StringLength(500, ErrorMessage = "요약은 최대 500자입니다.")]
    public string? Summary { get; set; }

    /// <summary>
    /// 카테고리 ID (선택)
    /// </summary>
    public long? CategoryId { get; set; }

    /// <summary>
    /// 태그 ID 목록 (선택)
    /// </summary>
    public List<long> TagIds { get; set; } = new();

    /// <summary>
    /// 썸네일 이미지 URL (선택)
    /// </summary>
    [Url(ErrorMessage = "올바른 URL 형식이 아닙니다.")]
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// SEO 키워드 (선택)
    /// </summary>
    [StringLength(500, ErrorMessage = "SEO 키워드는 최대 500자입니다.")]
    public string? SeoKeywords { get; set; }

    /// <summary>
    /// 메타 설명 (SEO용, 선택)
    /// </summary>
    [StringLength(160, ErrorMessage = "메타 설명은 최대 160자입니다.")]
    public string? MetaDescription { get; set; }

    /// <summary>
    /// 게시글 상태 (Draft, Published 등)
    /// 기본값: Draft
    /// </summary>
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// 추천글 여부
    /// </summary>
    public bool IsFeatured { get; set; }
}