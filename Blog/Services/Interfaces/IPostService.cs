using Blog.DTOs.Requests;
using Blog.DTOs.Responses;

namespace Blog.Services.Interfaces;

/// <summary>
/// 게시글 비즈니스 로직을 담당하는 서비스 인터페이스
/// Controller는 이 인터페이스만 의존 (구체적인 구현은 몰라도 됨)
///
/// .NET Framework에서도 이렇게 인터페이스 분리 패턴을 사용했죠?
/// </summary>
public interface IPostService
{
    /// <summary>
    /// 게시글 목록 조회 (페이지네이션, 필터링)
    /// </summary>
    /// <param name="page">페이지 번호 (1부터 시작)</param>
    /// <param name="pageSize">페이지당 아이템 수</param>
    /// <param name="status">상태 필터 (Draft, Published 등)</param>
    /// <param name="categoryId">카테고리 ID 필터</param>
    /// <param name="tagId">태그 ID 필터</param>
    /// <param name="searchQuery">검색어</param>
    /// <returns>페이지네이션된 게시글 목록</returns>
    Task<PaginatedResponse<PostListResponse>> GetPostsAsync(
        int page = 1,
        int pageSize = 10,
        string? status = null,
        long? categoryId = null,
        long? tagId = null,
        string? searchQuery = null);

    /// <summary>
    /// 게시글 단일 조회 (ID로)
    /// </summary>
    Task<PostResponse?> GetPostByIdAsync(long id);

    /// <summary>
    /// 게시글 단일 조회 (Slug로 - SEO 친화적 URL)
    /// 예: /blog/my-first-post (slug = "my-first-post")
    /// </summary>
    Task<PostResponse?> GetPostBySlugAsync(string slug);

    /// <summary>
    /// 게시글 생성
    /// </summary>
    /// <param name="request">생성 요청 DTO</param>
    /// <param name="userId">작성자 ID (현재 로그인한 사용자)</param>
    /// <returns>생성된 게시글</returns>
    Task<PostResponse> CreatePostAsync(CreatePostRequest request, long userId);

    /// <summary>
    /// 게시글 수정
    /// </summary>
    /// <param name="id">수정할 게시글 ID</param>
    /// <param name="request">수정 요청 DTO</param>
    /// <param name="userId">요청자 ID (권한 확인용)</param>
    /// <returns>수정된 게시글</returns>
    Task<PostResponse?> UpdatePostAsync(long id, UpdatePostRequest request, long userId);

    /// <summary>
    /// 게시글 삭제 (Soft Delete)
    /// </summary>
    /// <param name="id">삭제할 게시글 ID</param>
    /// <param name="userId">요청자 ID (권한 확인용)</param>
    /// <returns>성공 여부</returns>
    Task<bool> DeletePostAsync(long id, long userId);

    /// <summary>
    /// 조회수 증가
    /// 나중에 Redis로 최적화 예정 (Phase 2)
    /// </summary>
    Task IncrementViewCountAsync(long id);
}
