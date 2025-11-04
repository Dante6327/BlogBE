using Blog.DTOs.Requests;
using Blog.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers;

/// <summary>
/// 게시글 API 컨트롤러
///
/// .NET Framework Web API와 비교:
/// 1. [ApiController] 속성: 자동 모델 검증, 자동 400 BadRequest 응답
/// 2. [Route]: 라우팅 설정 (Web API와 유사)
/// 3. ControllerBase: MVC View 기능 제외 (API 전용)
/// </summary>
[ApiController]  // API 컨트롤러임을 명시
[Route("api/[controller]")]  // /api/posts 경로
public class PostsController : ControllerBase  // Controller가 아닌 ControllerBase (View 불필요)
{
    private readonly IPostService _postService;

    /// <summary>
    /// Dependency Injection: PostService 주입
    ///
    /// .NET Framework 방식:
    /// var service = new PostService(new BlogContext());
    ///
    /// .NET Core 방식:
    /// public PostsController(IPostService postService) { }
    /// Program.cs에서 services.AddScoped<IPostService, PostService>() 등록 필요
    /// </summary>
    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    /// <summary>
    /// 게시글 목록 조회
    /// GET /api/posts?page=1&pageSize=10&status=Published
    /// </summary>
    /// <param name="page">페이지 번호 (기본값: 1)</param>
    /// <param name="pageSize">페이지당 아이템 수 (기본값: 10)</param>
    /// <param name="status">상태 필터 (Draft, Published 등)</param>
    /// <param name="categoryId">카테고리 ID 필터</param>
    /// <param name="tagId">태그 ID 필터</param>
    /// <param name="searchQuery">검색어</param>
    /// <returns>페이지네이션된 게시글 목록</returns>
    [HttpGet]  // GET 메서드
    public async Task<IActionResult> GetPosts(
        [FromQuery] int page = 1,           // 쿼리 파라미터: ?page=1
        [FromQuery] int pageSize = 10,      // ?pageSize=10
        [FromQuery] string? status = null,
        [FromQuery] long? categoryId = null,
        [FromQuery] long? tagId = null,
        [FromQuery] string? searchQuery = null)
    {
        // 입력 검증
        if (page < 1)
        {
            return BadRequest(new { error = "페이지 번호는 1 이상이어야 합니다." });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "페이지 크기는 1~100 사이여야 합니다." });
        }

        // 서비스 호출
        var result = await _postService.GetPostsAsync(
            page, pageSize, status, categoryId, tagId, searchQuery);

        // 200 OK + 데이터 반환
        return Ok(result);
    }

    /// <summary>
    /// 게시글 단일 조회 (ID로)
    /// GET /api/posts/123
    /// </summary>
    [HttpGet("{id:long}")]  // {id} 파라미터, long 타입 제약
    public async Task<IActionResult> GetPostById(long id)
    {
        var post = await _postService.GetPostByIdAsync(id);

        if (post == null)
        {
            // 404 Not Found 반환
            return NotFound(new { error = $"게시글 ID {id}를 찾을 수 없습니다." });
        }

        // 조회수 증가 (비동기, 응답은 기다리지 않음)
        _ = _postService.IncrementViewCountAsync(id);

        return Ok(post);
    }

    /// <summary>
    /// 게시글 단일 조회 (Slug로 - SEO 친화적)
    /// GET /api/posts/slug/my-first-post
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetPostBySlug(string slug)
    {
        var post = await _postService.GetPostBySlugAsync(slug);

        if (post == null)
        {
            return NotFound(new { error = $"게시글 '{slug}'를 찾을 수 없습니다." });
        }

        // 조회수 증가
        _ = _postService.IncrementViewCountAsync(post.Id);

        return Ok(post);
    }

    /// <summary>
    /// 게시글 생성
    /// POST /api/posts
    /// Body: CreatePostRequest (JSON)
    /// </summary>
    /// <param name="request">생성 요청 DTO</param>
    /// <returns>생성된 게시글</returns>
    [HttpPost]
    // [Authorize]  // JWT 인증 필요 (나중에 추가)
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        // [ApiController]가 자동으로 모델 검증 수행
        // [Required], [StringLength] 등 위반 시 자동으로 400 BadRequest 반환

        // TODO: 현재 로그인한 사용자 ID 가져오기 (JWT에서)
        // 지금은 임시로 1번 사용자
        long currentUserId = 1;

        try
        {
            var post = await _postService.CreatePostAsync(request, currentUserId);

            // 201 Created + Location 헤더 + 생성된 데이터
            return CreatedAtAction(
                nameof(GetPostById),           // 생성된 리소스 조회 액션
                new { id = post.Id },          // 라우트 파라미터
                post);                         // 응답 본문
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error
            return StatusCode(500, new { error = "게시글 생성 중 오류가 발생했습니다.", detail = ex.Message });
        }
    }

    /// <summary>
    /// 게시글 수정
    /// PUT /api/posts/123
    /// Body: UpdatePostRequest (JSON)
    /// </summary>
    [HttpPut("{id:long}")]
    // [Authorize]
    public async Task<IActionResult> UpdatePost(long id, [FromBody] UpdatePostRequest request)
    {
        // TODO: 현재 로그인한 사용자 ID
        long currentUserId = 1;

        try
        {
            var post = await _postService.UpdatePostAsync(id, request, currentUserId);

            if (post == null)
            {
                return NotFound(new { error = $"게시글 ID {id}를 찾을 수 없습니다." });
            }

            return Ok(post);
        }
        catch (UnauthorizedAccessException ex)
        {
            // 403 Forbidden
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "게시글 수정 중 오류가 발생했습니다.", detail = ex.Message });
        }
    }

    /// <summary>
    /// 게시글 삭제 (Soft Delete)
    /// DELETE /api/posts/123
    /// </summary>
    [HttpDelete("{id:long}")]
    // [Authorize]
    public async Task<IActionResult> DeletePost(long id)
    {
        // TODO: 현재 로그인한 사용자 ID
        long currentUserId = 1;

        try
        {
            var result = await _postService.DeletePostAsync(id, currentUserId);

            if (!result)
            {
                return NotFound(new { error = $"게시글 ID {id}를 찾을 수 없습니다." });
            }

            // 204 No Content (삭제 성공, 응답 본문 없음)
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "게시글 삭제 중 오류가 발생했습니다.", detail = ex.Message });
        }
    }
}