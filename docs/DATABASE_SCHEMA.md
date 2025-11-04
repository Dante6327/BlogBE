# 🗄️ 데이터베이스 스키마 상세 문서

> **작성일**: 2025-11-04
> **DBMS**: PostgreSQL 16
> **ORM**: Entity Framework Core 9.0

---

## 📋 목차

1. [개요](#개요)
2. [전체 ER 다이어그램](#전체-er-다이어그램)
3. [테이블 상세 설계](#테이블-상세-설계)
4. [관계 정의](#관계-정의)
5. [인덱스 전략](#인덱스-전략)
6. [마이그레이션 가이드](#마이그레이션-가이드)

---

## 개요

### 설계 원칙

1. **정규화**: 3NF (Third Normal Form) 준수
2. **Soft Delete**: 중요 데이터는 물리적 삭제 대신 `deleted_at` 사용
3. **타임스탬프**: 모든 테이블에 `created_at`, `updated_at` 기본 포함
4. **외래키**: 참조 무결성 보장 (ON DELETE CASCADE/RESTRICT)
5. **인덱스**: 자주 조회되는 컬럼에 인덱스 설정

### 데이터베이스 정보

```yaml
DBMS: PostgreSQL 16
Character Set: UTF-8
Collation: en_US.UTF-8
Timezone: UTC
```

---

## 전체 ER 다이어그램

```
                              ┌──────────────────┐
                              │  UserSessions    │
                              │  (리프레시 토큰)   │
                              └────────┬─────────┘
                                       │ N:1
                                       │
┌──────────────┐                 ┌─────┴──────┐
│   Comments   │────────────────▶│   Users    │
│  (댓글)       │  N:1           │  (사용자)   │
└──────┬───────┘                 └─────┬──────┘
       │                               │ 1:N
       │ 1:N (self)                    │
       │                               ▼
       │                         ┌──────────────┐       ┌──────────────┐
       │                         │    Posts     │──────▶│  Categories  │
       │                         │   (게시글)    │  N:1  │  (카테고리)   │
       └────────────────────────▶│              │       └──────────────┘
                            N:1  └──────┬───────┘
                                        │
                      ┌─────────────────┼─────────────────┐
                      │                 │                 │
                      │ 1:N             │ N:N             │ 1:N
                      ▼                 ▼                 ▼
              ┌──────────────┐   ┌──────────────┐  ┌──────────────┐
              │PostRevisions │   │  PostTags    │  │  MediaFiles  │
              │(버전 히스토리)│   │  (중간테이블) │  │ (미디어파일)  │
              └──────────────┘   └──────┬───────┘  └──────────────┘
                                        │ N:1
                                        ▼
                                 ┌──────────────┐
                                 │     Tags     │
                                 │    (태그)     │
                                 └──────────────┘

                   ┌──────────────┐
                   │  Bookmarks   │
                   │  (북마크)     │
                   └──────┬───────┘
                          │ N:1
                          ├─────────▶ Users
                          └─────────▶ Posts
```

---

## 테이블 상세 설계

### 1. Users (사용자)

**목적**: 사용자 계정 정보 관리

```sql
CREATE TABLE users (
    -- Primary Key
    id BIGSERIAL PRIMARY KEY,

    -- 인증 정보
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    username VARCHAR(100) UNIQUE NOT NULL,

    -- 프로필 정보
    bio TEXT,
    avatar_url VARCHAR(500),

    -- 권한
    role VARCHAR(20) DEFAULT 'User' NOT NULL,
    -- User, Editor, Admin

    -- 타임스탬프
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    deleted_at TIMESTAMP NULL,

    -- 제약조건
    CONSTRAINT chk_users_role CHECK (role IN ('User', 'Editor', 'Admin'))
);

-- 인덱스
CREATE UNIQUE INDEX idx_users_email ON users(email) WHERE deleted_at IS NULL;
CREATE UNIQUE INDEX idx_users_username ON users(username) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_role ON users(role);
CREATE INDEX idx_users_created_at ON users(created_at DESC);
```

**컬럼 설명**:

| 컬럼 | 타입 | 설명 | 제약조건 |
|------|------|------|----------|
| id | BIGSERIAL | 사용자 ID | PK, AUTO_INCREMENT |
| email | VARCHAR(255) | 이메일 | UNIQUE, NOT NULL |
| password_hash | VARCHAR(255) | 해시된 비밀번호 | NOT NULL |
| username | VARCHAR(100) | 사용자명 | UNIQUE, NOT NULL |
| bio | TEXT | 자기소개 | NULL 가능 |
| avatar_url | VARCHAR(500) | 프로필 이미지 URL | NULL 가능 |
| role | VARCHAR(20) | 권한 | DEFAULT 'User' |
| created_at | TIMESTAMP | 생성일시 | DEFAULT NOW() |
| updated_at | TIMESTAMP | 수정일시 | DEFAULT NOW() |
| deleted_at | TIMESTAMP | 삭제일시 (Soft Delete) | NULL 가능 |

**EF Core 엔티티**:

```csharp
public class User
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
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
```

---

### 2. Posts (게시글)

**목적**: 블로그 게시글 저장

```sql
CREATE TABLE posts (
    -- Primary Key
    id BIGSERIAL PRIMARY KEY,

    -- Foreign Keys
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    category_id BIGINT REFERENCES categories(id) ON DELETE SET NULL,

    -- 콘텐츠
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL UNIQUE,
    content TEXT NOT NULL,
    summary VARCHAR(500),

    -- SEO 및 메타데이터
    thumbnail_url VARCHAR(500),
    reading_time_minutes INT DEFAULT 0,
    seo_keywords VARCHAR(500),
    meta_description VARCHAR(160),

    -- 상태 관리
    status VARCHAR(20) DEFAULT 'Draft' NOT NULL,
    view_count INT DEFAULT 0 NOT NULL,
    is_featured BOOLEAN DEFAULT FALSE NOT NULL,

    -- 타임스탬프
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    published_at TIMESTAMP NULL,
    deleted_at TIMESTAMP NULL,

    -- 제약조건
    CONSTRAINT chk_posts_status CHECK (status IN ('Draft', 'Published', 'Archived', 'Scheduled'))
);

-- 인덱스
CREATE UNIQUE INDEX idx_posts_slug ON posts(slug) WHERE deleted_at IS NULL;
CREATE INDEX idx_posts_user_id ON posts(user_id);
CREATE INDEX idx_posts_category_id ON posts(category_id);
CREATE INDEX idx_posts_status ON posts(status);
CREATE INDEX idx_posts_created_at ON posts(created_at DESC);
CREATE INDEX idx_posts_published_at ON posts(published_at DESC NULLS LAST);
CREATE INDEX idx_posts_status_published ON posts(status, published_at DESC) WHERE status = 'Published';

-- Full-Text Search 인덱스
CREATE INDEX idx_posts_search ON posts
USING GIN (to_tsvector('english', coalesce(title, '') || ' ' || coalesce(content, '')));
```

**컬럼 설명**:

| 컬럼 | 타입 | 설명 | 제약조건 |
|------|------|------|----------|
| id | BIGSERIAL | 게시글 ID | PK, AUTO_INCREMENT |
| user_id | BIGINT | 작성자 ID | FK → users.id, NOT NULL |
| category_id | BIGINT | 카테고리 ID | FK → categories.id, NULL 가능 |
| title | VARCHAR(255) | 제목 | NOT NULL |
| slug | VARCHAR(255) | URL 친화적 제목 | UNIQUE, NOT NULL |
| content | TEXT | 마크다운 본문 | NOT NULL |
| summary | VARCHAR(500) | 요약 | NULL 가능 |
| thumbnail_url | VARCHAR(500) | 썸네일 URL | NULL 가능 |
| reading_time_minutes | INT | 예상 읽기 시간 | DEFAULT 0 |
| seo_keywords | VARCHAR(500) | SEO 키워드 | NULL 가능 |
| meta_description | VARCHAR(160) | 메타 설명 | NULL 가능 |
| status | VARCHAR(20) | 글 상태 | DEFAULT 'Draft' |
| view_count | INT | 조회수 | DEFAULT 0 |
| is_featured | BOOLEAN | 추천글 여부 | DEFAULT FALSE |
| created_at | TIMESTAMP | 생성일시 | DEFAULT NOW() |
| updated_at | TIMESTAMP | 수정일시 | DEFAULT NOW() |
| published_at | TIMESTAMP | 발행일시 | NULL 가능 |
| deleted_at | TIMESTAMP | 삭제일시 | NULL 가능 |

**EF Core 엔티티**:

```csharp
public class Post
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }

    public string? ThumbnailUrl { get; set; }
    public int ReadingTimeMinutes { get; set; }
    public string? SeoKeywords { get; set; }
    public string? MetaDescription { get; set; }

    public PostStatus Status { get; set; } = PostStatus.Draft;
    public int ViewCount { get; set; }
    public bool IsFeatured { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // 네비게이션 속성
    public User User { get; set; } = null!;
    public Category? Category { get; set; }
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostRevision> Revisions { get; set; } = new List<PostRevision>();
}

public enum PostStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2,
    Scheduled = 3
}
```

---

### 3. Categories (카테고리)

**목적**: 게시글 분류

```sql
CREATE TABLE categories (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    slug VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    display_order INT DEFAULT 0 NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE UNIQUE INDEX idx_categories_slug ON categories(slug);
CREATE INDEX idx_categories_display_order ON categories(display_order);
```

**EF Core 엔티티**:

```csharp
public class Category
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
```

---

### 4. Tags (태그)

**목적**: 게시글 태깅

```sql
CREATE TABLE tags (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    slug VARCHAR(100) NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE UNIQUE INDEX idx_tags_slug ON tags(slug);
CREATE INDEX idx_tags_name ON tags(name);
```

**EF Core 엔티티**:

```csharp
public class Tag
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}
```

---

### 5. PostTags (다대다 중간 테이블)

**목적**: Posts ↔ Tags 다대다 관계

```sql
CREATE TABLE post_tags (
    post_id BIGINT NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    tag_id BIGINT NOT NULL REFERENCES tags(id) ON DELETE CASCADE,
    PRIMARY KEY (post_id, tag_id)
);

CREATE INDEX idx_post_tags_post_id ON post_tags(post_id);
CREATE INDEX idx_post_tags_tag_id ON post_tags(tag_id);
```

**EF Core 엔티티**:

```csharp
public class PostTag
{
    public long PostId { get; set; }
    public long TagId { get; set; }

    public Post Post { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
```

---

### 6. Comments (댓글)

**목적**: 게시글 댓글 (대댓글 지원)

```sql
CREATE TABLE comments (
    id BIGSERIAL PRIMARY KEY,
    post_id BIGINT NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    parent_comment_id BIGINT REFERENCES comments(id) ON DELETE RESTRICT,
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    deleted_at TIMESTAMP NULL
);

CREATE INDEX idx_comments_post_id ON comments(post_id);
CREATE INDEX idx_comments_user_id ON comments(user_id);
CREATE INDEX idx_comments_parent_id ON comments(parent_comment_id);
CREATE INDEX idx_comments_created_at ON comments(created_at DESC);
```

**대댓글 구조**:
- `parent_comment_id = NULL`: 최상위 댓글
- `parent_comment_id != NULL`: 대댓글

**EF Core 엔티티**:

```csharp
public class Comment
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public long UserId { get; set; }
    public long? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public Post Post { get; set; } = null!;
    public User User { get; set; } = null!;
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
```

---

### 7. MediaFiles (미디어 파일)

**목적**: 이미지/파일 메타데이터 관리

```sql
CREATE TABLE media_files (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    cdn_url VARCHAR(500),
    mime_type VARCHAR(100) NOT NULL,
    file_size_bytes BIGINT NOT NULL,
    width INT,
    height INT,
    alt_text VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    deleted_at TIMESTAMP NULL
);

CREATE INDEX idx_media_user_id ON media_files(user_id);
CREATE INDEX idx_media_created_at ON media_files(created_at DESC);
```

**EF Core 엔티티**:

```csharp
public class MediaFile
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? CdnUrl { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? AltText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public User User { get; set; } = null!;
}
```

---

### 8. PostRevisions (글 버전 히스토리)

**목적**: 게시글 수정 히스토리 추적

```sql
CREATE TABLE post_revisions (
    id BIGSERIAL PRIMARY KEY,
    post_id BIGINT NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    summary VARCHAR(500),
    revision_number INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX idx_post_revisions_post_id ON post_revisions(post_id);
CREATE INDEX idx_post_revisions_created_at ON post_revisions(created_at DESC);
```

**EF Core 엔티티**:

```csharp
public class PostRevision
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public int RevisionNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Post Post { get; set; } = null!;
}
```

---

### 9. UserSessions (리프레시 토큰)

**목적**: JWT 리프레시 토큰 관리

```sql
CREATE TABLE user_sessions (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    refresh_token VARCHAR(500) NOT NULL UNIQUE,
    device_info VARCHAR(500),
    ip_address VARCHAR(45),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    is_revoked BOOLEAN DEFAULT FALSE NOT NULL
);

CREATE UNIQUE INDEX idx_sessions_refresh_token ON user_sessions(refresh_token);
CREATE INDEX idx_sessions_user_id ON user_sessions(user_id);
CREATE INDEX idx_sessions_expires_at ON user_sessions(expires_at);
```

**EF Core 엔티티**:

```csharp
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

    public User User { get; set; } = null!;
}
```

---

### 10. Bookmarks (북마크)

**목적**: 사용자의 게시글 북마크

```sql
CREATE TABLE bookmarks (
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    post_id BIGINT NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    PRIMARY KEY (user_id, post_id)
);

CREATE INDEX idx_bookmarks_user_id ON bookmarks(user_id);
CREATE INDEX idx_bookmarks_post_id ON bookmarks(post_id);
CREATE INDEX idx_bookmarks_created_at ON bookmarks(created_at DESC);
```

**EF Core 엔티티**:

```csharp
public class Bookmark
{
    public long UserId { get; set; }
    public long PostId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}
```

---

## 관계 정의

### 1:N 관계

| 부모 테이블 | 자식 테이블 | 관계 | 삭제 정책 |
|------------|------------|------|-----------|
| users | posts | 1:N | CASCADE |
| users | comments | 1:N | CASCADE |
| users | media_files | 1:N | CASCADE |
| users | user_sessions | 1:N | CASCADE |
| posts | comments | 1:N | CASCADE |
| posts | post_revisions | 1:N | CASCADE |
| categories | posts | 1:N | SET NULL |
| comments | comments | 1:N (self) | RESTRICT |

### N:N 관계

| 테이블 1 | 테이블 2 | 중간 테이블 |
|---------|---------|------------|
| posts | tags | post_tags |
| users | posts | bookmarks |

---

## 인덱스 전략

### 1. 기본 인덱스 (자동 생성)
- Primary Key: 모든 테이블의 `id` 컬럼
- Unique: `email`, `username`, `slug` 등

### 2. 외래키 인덱스
```sql
-- 자주 JOIN되는 외래키에 인덱스
CREATE INDEX idx_posts_user_id ON posts(user_id);
CREATE INDEX idx_posts_category_id ON posts(category_id);
CREATE INDEX idx_comments_post_id ON comments(post_id);
```

### 3. 복합 인덱스
```sql
-- 상태 + 날짜 (자주 함께 조회)
CREATE INDEX idx_posts_status_published ON posts(status, published_at DESC)
WHERE status = 'Published';
```

### 4. 부분 인덱스 (Partial Index)
```sql
-- Soft Delete가 적용된 테이블
CREATE UNIQUE INDEX idx_users_email ON users(email)
WHERE deleted_at IS NULL;

CREATE UNIQUE INDEX idx_posts_slug ON posts(slug)
WHERE deleted_at IS NULL;
```

### 5. Full-Text Search 인덱스
```sql
-- GIN 인덱스 (PostgreSQL)
CREATE INDEX idx_posts_search ON posts
USING GIN (to_tsvector('english', coalesce(title, '') || ' ' || coalesce(content, '')));
```

### 인덱스 사용 예시

```sql
-- 발행된 글 목록 (복합 인덱스 사용)
SELECT * FROM posts
WHERE status = 'Published'
ORDER BY published_at DESC
LIMIT 10;

-- 전문 검색 (GIN 인덱스 사용)
SELECT * FROM posts
WHERE to_tsvector('english', title || ' ' || content) @@ to_tsquery('english', 'ASP.NET & Core');
```

---

## 마이그레이션 가이드

### 초기 마이그레이션 생성

```bash
# 마이그레이션 생성
dotnet ef migrations add InitialCreate

# 마이그레이션 적용
dotnet ef database update

# 특정 마이그레이션으로 롤백
dotnet ef database update PreviousMigrationName

# 마이그레이션 스크립트 생성 (SQL 파일)
dotnet ef migrations script -o migrations.sql
```

### 개발 워크플로우

1. **모델 변경** → `Models/Post.cs` 수정
2. **마이그레이션 생성** → `dotnet ef migrations add AddNewColumn`
3. **검토** → `Migrations/XXX_AddNewColumn.cs` 확인
4. **적용** → `dotnet ef database update`

### 프로덕션 배포 시

```bash
# 1. SQL 스크립트 생성
dotnet ef migrations script --idempotent -o migration_v2.sql

# 2. SQL 스크립트 검토 후 수동 실행
psql -U blog_user -d blog_db -f migration_v2.sql

# 3. 백업 먼저!
pg_dump -U blog_user blog_db > backup_before_migration.sql
```

---

## 데이터 샘플

### 샘플 데이터 삽입

```sql
-- 사용자
INSERT INTO users (email, password_hash, username, role)
VALUES ('admin@blog.com', '$2a$11$...', 'admin', 'Admin');

-- 카테고리
INSERT INTO categories (name, slug, description, display_order)
VALUES
    ('Backend', 'backend', '.NET, C#, API 개발', 1),
    ('Frontend', 'frontend', 'React, Next.js', 2),
    ('DevOps', 'devops', 'Docker, CI/CD', 3);

-- 태그
INSERT INTO tags (name, slug)
VALUES
    ('ASP.NET Core', 'aspnet-core'),
    ('PostgreSQL', 'postgresql'),
    ('Next.js', 'nextjs');

-- 게시글
INSERT INTO posts (user_id, category_id, title, slug, content, status, published_at)
VALUES (
    1,
    1,
    'ASP.NET Core 시작하기',
    'getting-started-aspnet-core',
    '# ASP.NET Core 소개\n\n...',
    'Published',
    NOW()
);
```

---

## 성능 최적화 팁

### 1. N+1 문제 방지

```csharp
// ❌ N+1 발생
var posts = await _context.Posts.ToListAsync();
foreach (var post in posts)
{
    var author = post.User; // 각 post마다 쿼리 실행
}

// ✅ Include 사용
var posts = await _context.Posts
    .Include(p => p.User)
    .Include(p => p.Category)
    .Include(p => p.PostTags)
        .ThenInclude(pt => pt.Tag)
    .ToListAsync();
```

### 2. AsNoTracking 사용

```csharp
// 읽기 전용 쿼리 (변경 추적 불필요)
var posts = await _context.Posts
    .AsNoTracking()
    .Where(p => p.Status == PostStatus.Published)
    .ToListAsync();
```

### 3. 페이지네이션

```csharp
var posts = await _context.Posts
    .Where(p => p.Status == PostStatus.Published)
    .OrderByDescending(p => p.PublishedAt)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### 4. Select Projection

```csharp
// ❌ 전체 엔티티 로드
var posts = await _context.Posts.ToListAsync();

// ✅ 필요한 필드만 선택
var posts = await _context.Posts
    .Select(p => new {
        p.Id,
        p.Title,
        p.Summary,
        p.PublishedAt
    })
    .ToListAsync();
```

---

## 백업 및 복구

### 수동 백업

```bash
# 전체 DB 백업
pg_dump -U blog_user -h localhost blog_db > backup_$(date +%Y%m%d).sql

# 특정 테이블만 백업
pg_dump -U blog_user -h localhost -t posts blog_db > posts_backup.sql

# 압축 백업
pg_dump -U blog_user -h localhost blog_db | gzip > backup_$(date +%Y%m%d).sql.gz
```

### 복구

```bash
# SQL 파일에서 복구
psql -U blog_user -h localhost blog_db < backup.sql

# 압축 파일에서 복구
gunzip -c backup.sql.gz | psql -U blog_user -h localhost blog_db
```

### 자동 백업 (Cron)

```bash
# /etc/cron.d/blog-backup
0 2 * * * postgres pg_dump blog_db | gzip > /backups/db_$(date +\%Y\%m\%d).sql.gz
0 3 * * 0 find /backups -name "*.sql.gz" -mtime +30 -delete
```

---

## 참고 자료

- [PostgreSQL 공식 문서](https://www.postgresql.org/docs/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [PostgreSQL Index Types](https://www.postgresql.org/docs/current/indexes-types.html)
- [Full-Text Search in PostgreSQL](https://www.postgresql.org/docs/current/textsearch.html)

---

**문서 버전**: 1.0
**최종 수정**: 2025-11-04