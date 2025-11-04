namespace Blog.DTOs.Responses;

/// <summary>
/// 페이지네이션 정보를 포함한 응답
/// 목록 조회 시 "전체 몇 개, 현재 몇 페이지" 정보 필요
/// </summary>
/// <typeparam name="T">응답 데이터 타입 (PostListResponse 등)</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// 실제 데이터 목록
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// 페이지네이션 정보
    /// </summary>
    public PaginationMetadata Pagination { get; set; } = null!;
}

/// <summary>
/// 페이지네이션 메타데이터
/// </summary>
public class PaginationMetadata
{
    public int CurrentPage { get; set; }      // 현재 페이지 번호
    public int PageSize { get; set; }         // 페이지당 아이템 수
    public int TotalPages { get; set; }       // 전체 페이지 수
    public long TotalItems { get; set; }      // 전체 아이템 수
    public bool HasPrevious { get; set; }     // 이전 페이지 존재 여부
    public bool HasNext { get; set; }         // 다음 페이지 존재 여부
}