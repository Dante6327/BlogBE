using Blog.Data;
using Blog.DTOs.Requests;
using Blog.DTOs.Responses;
using Blog.Model;
using Blog.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Implementations;

/// <summary>
/// 게시글 비즈니스 로직 구현
///
/// .NET Framework에서의 차이점:
/// 1. 생성자로 DbContext를 주입받음 (new BlogContext() 대신)
/// 2. async/await를 적극 활용 (I/O 작업 시 스레드를 차단하지 않음)
/// 3. LINQ는 동일하게 사용
/// </summary>
public class PostService : IPostService
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// 생성자: Dependency Injection으로 DbContext 주입
    ///
    /// .NET Framework 방식:
    /// var context = new BlogContext();  // 직접 생성
    ///
    /// .NET Core 방식:
    /// public PostService(ApplicationDbContext context) { } // 주입받음
    /// </summary>
    public PostService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 게시글 목록 조회 (페이지네이션 + 필터링)
    /// </summary>
    public async Task<PaginatedResponse<PostListResponse>> GetPostsAsync(
        int page = 1,
        int pageSize = 10,
        string? status = null,
        long? categoryId = null,
        long? tagId = null,
        string? searchQuery = null)
    {
        // LINQ 쿼리 시작 (.NET Framework와 동일)
        var query = _context.Posts
            .Include(p => p.User)        // JOIN Users (Eager Loading)
            .Include(p => p.Category)    // JOIN Categories
            .Include(p => p.PostTags)    // JOIN PostTags
                .ThenInclude(pt => pt.Tag)  // JOIN Tags
            .AsQueryable();  // IQueryable로 변환 (필터 추가 가능)

        // 동적 필터 적용 (.NET Framework와 동일)
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(p => p.Status.ToString() == status);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (tagId.HasValue)
        {
            // N:N 관계 필터링
            query = query.Where(p => p.PostTags.Any(pt => pt.TagId == tagId.Value));
        }

        if (!string.IsNullOrEmpty(searchQuery))
        {
            // 제목 또는 내용에서 검색
            query = query.Where(p =>
                p.Title.Contains(searchQuery) ||
                p.Content.Contains(searchQuery));
        }

        // 총 개수 계산 (페이지네이션용)
        // .NET Framework: query.Count();
        // .NET Core: await query.CountAsync(); (비동기)
        var totalItems = await query.CountAsync();

        // 페이지네이션 적용 + 정렬
        var posts = await query
            .OrderByDescending(p => p.CreatedAt)  // 최신순 정렬
            .Skip((page - 1) * pageSize)          // (page-1) * pageSize 건너뛰기
            .Take(pageSize)                       // pageSize만큼 가져오기
            .ToListAsync();  // 비동기로 DB에서 실제 조회

        // Entity → DTO 변환
        // AutoMapper를 사용할 수도 있지만, 지금은 수동 매핑
        var items = posts.Select(p => new PostListResponse
        {
            Id = p.Id,
            Title = p.Title,
            Slug = p.Slug,
            Summary = p.Summary,
            ThumbnailUrl = p.ThumbnailUrl,
            ReadingTimeMinutes = p.ReadingTimeMinutes,
            Status = p.Status.ToString(),  // Enum → string
            ViewCount = p.ViewCount,
            IsFeatured = p.IsFeatured,

            // 작성자 정보 (User 엔티티에서 필요한 것만)
            Author = new AuthorDto
            {
                Id = p.User.Id,
                Username = p.User.Username,
                AvatarUrl = p.User.AvatarUrl
            },

            // 카테고리 정보 (nullable)
            Category = p.Category == null ? null : new CategoryDto
            {
                Id = p.Category.Id,
                Name = p.Category.Name,
                Slug = p.Category.Slug
            },

            // 태그 목록
            Tags = p.PostTags.Select(pt => new TagDto
            {
                Id = pt.Tag.Id,
                Name = pt.Tag.Name,
                Slug = pt.Tag.Slug
            }).ToList(),

            CreatedAt = p.CreatedAt,
            PublishedAt = p.PublishedAt
        }).ToList();

        // 페이지네이션 메타데이터 계산
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PaginatedResponse<PostListResponse>
        {
            Items = items,
            Pagination = new PaginationMetadata
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                HasPrevious = page > 1,
                HasNext = page < totalPages
            }
        };
    }

    /// <summary>
    /// 게시글 단일 조회 (ID로)
    /// </summary>
    public async Task<PostResponse?> GetPostByIdAsync(long id)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Category)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);  // WHERE Id = @id LIMIT 1

        if (post == null)
        {
            return null;  // 없으면 null 반환 (Controller에서 404 처리)
        }

        return MapToPostResponse(post);
    }

    /// <summary>
    /// 게시글 단일 조회 (Slug로 - SEO 친화적)
    /// 예: GET /blog/my-first-post
    /// </summary>
    public async Task<PostResponse?> GetPostBySlugAsync(string slug)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Category)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (post == null)
        {
            return null;
        }

        return MapToPostResponse(post);
    }

    /// <summary>
    /// 게시글 생성
    /// </summary>
    public async Task<PostResponse> CreatePostAsync(CreatePostRequest request, long userId)
    {
        // Slug 생성 (제목 → URL 친화적 문자열)
        // 예: "안녕하세요 첫 글입니다!" → "hello-first-post"
        var slug = GenerateSlug(request.Title);

        // Slug 중복 체크 (UNIQUE 제약)
        var existingSlug = await _context.Posts
            .AnyAsync(p => p.Slug == slug);

        if (existingSlug)
        {
            // 중복이면 숫자 추가: my-post-1, my-post-2
            slug = await GenerateUniqueSlugAsync(slug);
        }

        // 읽기 시간 자동 계산 (평균 200 단어/분)
        var wordCount = request.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var readingTimeMinutes = Math.Max(1, wordCount / 200);

        // Entity 생성
        var post = new Post
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Slug = slug,
            Content = request.Content,
            Summary = request.Summary,
            ThumbnailUrl = request.ThumbnailUrl,
            ReadingTimeMinutes = readingTimeMinutes,
            SeoKeywords = request.SeoKeywords,
            MetaDescription = request.MetaDescription,
            Status = Enum.Parse<PostStatus>(request.Status),  // string → Enum
            IsFeatured = request.IsFeatured,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Published 상태면 PublishedAt 설정
        if (post.Status == PostStatus.Published)
        {
            post.PublishedAt = DateTime.UtcNow;
        }

        // DB에 추가
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();  // INSERT 실행

        // 태그 연결 (N:N 관계)
        if (request.TagIds.Any())
        {
            var postTags = request.TagIds.Select(tagId => new PostTag
            {
                PostId = post.Id,
                TagId = tagId
            }).ToList();

            _context.PostTags.AddRange(postTags);
            await _context.SaveChangesAsync();
        }

        // 생성된 게시글 다시 조회 (Include 적용)
        return (await GetPostByIdAsync(post.Id))!;
    }

    /// <summary>
    /// 게시글 수정
    /// </summary>
    public async Task<PostResponse?> UpdatePostAsync(long id, UpdatePostRequest request, long userId)
    {
        var post = await _context.Posts
            .Include(p => p.PostTags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return null;
        }

        // 권한 체크: 작성자 본인만 수정 가능
        // (나중에 Role.Admin은 모든 글 수정 가능하도록 추가)
        if (post.UserId != userId)
        {
            throw new UnauthorizedAccessException("본인의 게시글만 수정할 수 있습니다.");
        }

        // 제목이 변경되면 Slug도 재생성
        if (post.Title != request.Title)
        {
            var newSlug = GenerateSlug(request.Title);
            if (await _context.Posts.AnyAsync(p => p.Slug == newSlug && p.Id != id))
            {
                newSlug = await GenerateUniqueSlugAsync(newSlug);
            }
            post.Slug = newSlug;
        }

        // 읽기 시간 재계산
        var wordCount = request.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        post.ReadingTimeMinutes = Math.Max(1, wordCount / 200);

        // 필드 업데이트
        post.Title = request.Title;
        post.Content = request.Content;
        post.Summary = request.Summary;
        post.CategoryId = request.CategoryId;
        post.ThumbnailUrl = request.ThumbnailUrl;
        post.SeoKeywords = request.SeoKeywords;
        post.MetaDescription = request.MetaDescription;
        post.IsFeatured = request.IsFeatured;
        post.UpdatedAt = DateTime.UtcNow;

        // 상태 변경 처리
        var newStatus = Enum.Parse<PostStatus>(request.Status);
        if (post.Status != newStatus)
        {
            post.Status = newStatus;
            if (newStatus == PostStatus.Published && post.PublishedAt == null)
            {
                post.PublishedAt = DateTime.UtcNow;
            }
        }

        // 기존 태그 삭제 후 재추가
        _context.PostTags.RemoveRange(post.PostTags);

        if (request.TagIds.Any())
        {
            var postTags = request.TagIds.Select(tagId => new PostTag
            {
                PostId = post.Id,
                TagId = tagId
            }).ToList();

            _context.PostTags.AddRange(postTags);
        }

        await _context.SaveChangesAsync();

        return await GetPostByIdAsync(post.Id);
    }

    /// <summary>
    /// 게시글 삭제 (Soft Delete)
    /// </summary>
    public async Task<bool> DeletePostAsync(long id, long userId)
    {
        var post = await _context.Posts.FindAsync(id);

        if (post == null)
        {
            return false;
        }

        // 권한 체크
        if (post.UserId != userId)
        {
            throw new UnauthorizedAccessException("본인의 게시글만 삭제할 수 있습니다.");
        }

        // Soft Delete: DeletedAt 설정
        post.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// 조회수 증가
    /// 나중에 Redis로 최적화 (Phase 2)
    /// </summary>
    public async Task IncrementViewCountAsync(long id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post != null)
        {
            post.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    // ========== Private Helper Methods ==========

    /// <summary>
    /// Entity → PostResponse 변환
    /// </summary>
    private PostResponse MapToPostResponse(Post post)
    {
        return new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Content = post.Content,
            Summary = post.Summary,
            ThumbnailUrl = post.ThumbnailUrl,
            ReadingTimeMinutes = post.ReadingTimeMinutes,
            Status = post.Status.ToString(),
            ViewCount = post.ViewCount,
            IsFeatured = post.IsFeatured,

            Author = new AuthorDto
            {
                Id = post.User.Id,
                Username = post.User.Username,
                AvatarUrl = post.User.AvatarUrl
            },

            Category = post.Category == null ? null : new CategoryDto
            {
                Id = post.Category.Id,
                Name = post.Category.Name,
                Slug = post.Category.Slug
            },

            Tags = post.PostTags.Select(pt => new TagDto
            {
                Id = pt.Tag.Id,
                Name = pt.Tag.Name,
                Slug = pt.Tag.Slug
            }).ToList(),

            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            PublishedAt = post.PublishedAt
        };
    }

    /// <summary>
    /// 제목 → Slug 변환
    /// 예: "Hello World!" → "hello-world"
    /// </summary>
    private string GenerateSlug(string title)
    {
        // 간단한 구현 (한글은 별도 처리 필요)
        return title
            .ToLower()
            .Replace(" ", "-")
            .Replace("!", "")
            .Replace("?", "")
            .Replace(".", "")
            .Replace(",", "");
    }

    /// <summary>
    /// 중복되지 않는 Slug 생성 (숫자 suffix 추가)
    /// </summary>
    private async Task<string> GenerateUniqueSlugAsync(string baseSlug)
    {
        var counter = 1;
        var newSlug = $"{baseSlug}-{counter}";

        while (await _context.Posts.AnyAsync(p => p.Slug == newSlug))
        {
            counter++;
            newSlug = $"{baseSlug}-{counter}";
        }

        return newSlug;
    }
}
