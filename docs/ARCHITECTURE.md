# 🏗️ 블로그 플랫폼 아키텍처 문서

> **작성일**: 2025-11-04
> **버전**: 2.0 (개선판)
> **상태**: 기획 완료 / 개발 진행 중

---

## 📋 목차

1. [프로젝트 개요](#프로젝트-개요)
2. [기술 스택](#기술-스택)
3. [시스템 아키텍처](#시스템-아키텍처)
4. [데이터베이스 설계](#데이터베이스-설계)
5. [API 설계](#api-설계)
6. [보안 전략](#보안-전략)
7. [성능 최적화](#성능-최적화)
8. [배포 전략](#배포-전략)
9. [개발 로드맵](#개발-로드맵)

---

## 프로젝트 개요

### 목표
- **포트폴리오**: 실제 운영 가능한 블로그 플랫폼
- **SEO 최적화**: 검색 엔진 친화적 구조
- **수익화**: 광고/제휴 마케팅 가능
- **확장성**: 점진적 기능 추가 가능한 설계

### 주요 특징
- ✅ 마크다운 기반 에디터
- ✅ 실시간 프리뷰
- ✅ SEO 최적화 (Meta, OG, Sitemap)
- ✅ 이미지 업로드 및 관리
- ✅ 댓글 시스템 (대댓글 지원)
- ✅ 태그/카테고리 분류
- ✅ 전문 검색 (Full-Text Search)
- ✅ 조회수 추적
- ✅ 글 버전 히스토리
- ✅ 북마크 기능

### 개발 환경
- **OS**: macOS (개발) / Ubuntu (운영)
- **배포**: 개인 홈서버
- **예상 기간**: 8-10주

---

## 기술 스택

### Backend

```yaml
Runtime: .NET 9.0
Framework: ASP.NET Core Web API
ORM: Entity Framework Core 9.0
Database: PostgreSQL 16
Caching: Redis 7+ (Phase 2)
Logging: Serilog
Validation: FluentValidation
Authentication: JWT (HttpOnly Cookies)
Rate Limiting: ASP.NET Core Built-in
Documentation: Swagger/OpenAPI
```

**선택 이유**:
- .NET 9.0: 최신 성능 개선, Native AOT 지원
- PostgreSQL: JSONB 지원, Full-Text Search, 안정성
- Entity Framework Core: 강력한 ORM, 마이그레이션 관리
- Redis: 고성능 캐싱, 세션 관리

### Frontend

```yaml
Framework: Next.js 15 (App Router)
Language: TypeScript
State Management: Zustand
Data Fetching: TanStack Query (React Query)
Forms: React Hook Form + Zod
Markdown: react-markdown + remark/rehype
Styling: Tailwind CSS + shadcn/ui
Icons: Lucide React
Image Optimization: next/image
```

**선택 이유**:
- Next.js 15: SSR/SSG 지원, SEO 최적화, App Router
- TypeScript: 타입 안정성
- TanStack Query: 강력한 캐싱, 자동 재검증
- Tailwind + shadcn/ui: 빠른 UI 개발

### DevOps

```yaml
Container: Docker
Orchestration: Docker Compose
CI/CD: GitHub Actions
Reverse Proxy: Nginx
SSL: Let's Encrypt
Version Control: Git + GitHub
Monitoring: Prometheus + Grafana (선택)
Error Tracking: Sentry (선택)
```

---

## 시스템 아키텍처

### 전체 아키텍처

```
┌─────────────────────────────────────────────────────────┐
│                    Client (Browser)                      │
│              Next.js 15 (SSR/SSG + CSR)                 │
└────────────────────┬────────────────────────────────────┘
                     │ HTTPS
                     ▼
┌─────────────────────────────────────────────────────────┐
│                   Nginx (Reverse Proxy)                  │
│            SSL Termination + Rate Limiting              │
└─────────┬──────────────────────────────────────┬────────┘
          │                                       │
          ▼                                       ▼
┌──────────────────────┐              ┌──────────────────────┐
│   Next.js Server     │              │   ASP.NET Core API   │
│   (Port 3000)        │              │   (Port 5000)        │
└──────────────────────┘              └──────────┬───────────┘
                                                 │
                     ┌───────────────────────────┼───────────────┐
                     ▼                           ▼               ▼
          ┌──────────────────┐       ┌──────────────┐  ┌─────────────┐
          │   PostgreSQL     │       │    Redis     │  │  File       │
          │   (Port 5432)    │       │  (Port 6379) │  │  Storage    │
          └──────────────────┘       └──────────────┘  └─────────────┘
```

### 백엔드 레이어 아키텍처

```
┌─────────────────────────────────────────────────────────┐
│                    Controllers                           │
│         (API Endpoints, Request/Response)               │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                   Services Layer                         │
│          (Business Logic, Validation)                   │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                Repository Pattern                        │
│              (Data Access Abstraction)                  │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│              Entity Framework Core                       │
│                 (ORM + Migrations)                      │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
              ┌──────────────┐
              │  PostgreSQL  │
              └──────────────┘
```

### 프로젝트 구조

```
Blog/
├── Blog.API/                      # ASP.NET Core Web API
│   ├── Controllers/               # API 컨트롤러
│   │   ├── AuthController.cs
│   │   ├── PostsController.cs
│   │   ├── CategoriesController.cs
│   │   ├── TagsController.cs
│   │   ├── CommentsController.cs
│   │   └── MediaController.cs
│   ├── Services/                  # 비즈니스 로직
│   │   ├── Interfaces/
│   │   │   ├── IAuthService.cs
│   │   │   ├── IPostService.cs
│   │   │   ├── ICategoryService.cs
│   │   │   └── IMediaService.cs
│   │   └── Implementations/
│   │       ├── AuthService.cs
│   │       ├── PostService.cs
│   │       └── MediaService.cs
│   ├── Models/                    # 엔티티 모델
│   │   ├── User.cs
│   │   ├── Post.cs
│   │   ├── Category.cs
│   │   ├── Tag.cs
│   │   ├── Comment.cs
│   │   ├── MediaFile.cs
│   │   ├── PostRevision.cs
│   │   ├── UserSession.cs
│   │   └── Bookmark.cs
│   ├── DTOs/                      # 데이터 전송 객체
│   │   ├── Requests/
│   │   └── Responses/
│   ├── Data/                      # 데이터 컨텍스트
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations/
│   ├── Middlewares/               # 미들웨어
│   │   ├── ErrorHandlingMiddleware.cs
│   │   ├── AuthenticationMiddleware.cs
│   │   └── RequestLoggingMiddleware.cs
│   ├── Utils/                     # 유틸리티
│   │   ├── JwtTokenGenerator.cs
│   │   ├── SlugGenerator.cs
│   │   ├── PasswordHasher.cs
│   │   └── ImageProcessor.cs
│   ├── Validators/                # FluentValidation
│   │   ├── CreatePostValidator.cs
│   │   └── RegisterUserValidator.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
│
├── Blog.Tests/                    # 테스트 프로젝트
│   ├── Unit/
│   ├── Integration/
│   └── E2E/
│
└── docs/                          # 문서
    ├── ARCHITECTURE.md            # 이 파일
    ├── DATABASE_SCHEMA.md         # DB 스키마
    ├── API_SPEC.md                # API 명세
    └── ROADMAP.md                 # 개발 로드맵
```

---

## 데이터베이스 설계

### ER 다이어그램 (개념도)

```
┌──────────────┐       ┌──────────────┐       ┌──────────────┐
│    Users     │───────│    Posts     │───────│  Categories  │
│              │ 1:N   │              │ N:1   │              │
│  - id        │       │  - id        │       │  - id        │
│  - email     │       │  - title     │       │  - name      │
│  - username  │       │  - content   │       │  - slug      │
│  - role      │       │  - slug      │       └──────────────┘
└──────┬───────┘       │  - status    │
       │               └──────┬───────┘
       │                      │
       │                      │ N:N
       │               ┌──────┴───────┐
       │               │   PostTags   │       ┌──────────────┐
       │               │              │───────│     Tags     │
       │               │  - post_id   │  N:1  │              │
       │               │  - tag_id    │       │  - id        │
       │               └──────────────┘       │  - name      │
       │                                      └──────────────┘
       │
       │ 1:N           ┌──────────────┐
       └───────────────│   Comments   │
                       │              │
                       │  - id        │
                       │  - post_id   │
                       │  - content   │
                       │  - parent_id │ (대댓글)
                       └──────────────┘
```

### 주요 테이블 설계

#### Users (사용자)
```sql
CREATE TABLE users (
    id BIGSERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    username VARCHAR(100) UNIQUE NOT NULL,
    bio TEXT,
    avatar_url VARCHAR(500),
    role VARCHAR(20) DEFAULT 'User', -- User, Editor, Admin
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP NULL
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_username ON users(username);
```

#### Posts (게시글)
```sql
CREATE TABLE posts (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    category_id BIGINT REFERENCES categories(id),

    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL UNIQUE,
    content TEXT NOT NULL,
    summary VARCHAR(500),

    thumbnail_url VARCHAR(500),
    reading_time_minutes INT DEFAULT 0,
    seo_keywords VARCHAR(500),
    meta_description VARCHAR(160),

    status VARCHAR(20) DEFAULT 'Draft', -- Draft, Published, Archived, Scheduled
    view_count INT DEFAULT 0,
    is_featured BOOLEAN DEFAULT FALSE,

    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    published_at TIMESTAMP NULL,
    deleted_at TIMESTAMP NULL
);

CREATE INDEX idx_posts_slug ON posts(slug);
CREATE INDEX idx_posts_status ON posts(status);
CREATE INDEX idx_posts_user_id ON posts(user_id);
CREATE INDEX idx_posts_category_id ON posts(category_id);
CREATE INDEX idx_posts_created_at ON posts(created_at DESC);
CREATE INDEX idx_posts_published_at ON posts(published_at DESC);
CREATE INDEX idx_posts_status_created ON posts(status, created_at DESC);

-- PostgreSQL Full-Text Search 인덱스
CREATE INDEX idx_posts_search ON posts
USING GIN (to_tsvector('english', title || ' ' || content));
```

#### Categories (카테고리)
```sql
CREATE TABLE categories (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    slug VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    display_order INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_categories_slug ON categories(slug);
```

#### Tags (태그)
```sql
CREATE TABLE tags (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    slug VARCHAR(100) NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_tags_slug ON tags(slug);
```

#### PostTags (다대다 중간 테이블)
```sql
CREATE TABLE post_tags (
    post_id BIGINT NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    tag_id BIGINT NOT NULL REFERENCES tags(id) ON DELETE CASCADE,
    PRIMARY KEY (post_id, tag_id)
);

CREATE INDEX idx_post_tags_post_id ON post_tags(post_id);
CREATE INDEX idx_post_tags_tag_id ON post_tags(tag_id);
```

#### Comments (댓글)
```sql
CREATE TABLE comments (
    id BIGSERIAL PRIMARY KEY,
    post_id BIGINT NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    parent_comment_id BIGINT REFERENCES comments(id) ON DELETE RESTRICT,
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP NULL
);

CREATE INDEX idx_comments_post_id ON comments(post_id);
CREATE INDEX idx_comments_user_id ON comments(user_id);
CREATE INDEX idx_comments_parent_id ON comments(parent_comment_id);
```

#### MediaFiles (미디어 파일)
```sql
CREATE TABLE media_files (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id),
    file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    cdn_url VARCHAR(500),
    mime_type VARCHAR(100) NOT NULL,
    file_size_bytes BIGINT NOT NULL,
    width INT,
    height INT,
    alt_text VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP NULL
);

CREATE INDEX idx_media_user_id ON media_files(user_id);
```

#### PostRevisions (글 버전 히스토리)
```sql
CREATE TABLE post_revisions (
    id BIGSERIAL PRIMARY KEY,
    post_id BIGINT NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    summary VARCHAR(500),
    revision_number INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_post_revisions_post_id ON post_revisions(post_id);
```

#### UserSessions (리프레시 토큰)
```sql
CREATE TABLE user_sessions (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    refresh_token VARCHAR(500) NOT NULL UNIQUE,
    device_info VARCHAR(500),
    ip_address VARCHAR(45),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP NOT NULL,
    is_revoked BOOLEAN DEFAULT FALSE
);

CREATE INDEX idx_sessions_refresh_token ON user_sessions(refresh_token);
CREATE INDEX idx_sessions_user_id ON user_sessions(user_id);
CREATE INDEX idx_sessions_expires_at ON user_sessions(expires_at);
```

#### Bookmarks (북마크)
```sql
CREATE TABLE bookmarks (
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    post_id BIGINT NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, post_id)
);

CREATE INDEX idx_bookmarks_user_id ON bookmarks(user_id);
CREATE INDEX idx_bookmarks_post_id ON bookmarks(post_id);
```

---

## API 설계

### 인증 (Authentication)

```
POST   /api/auth/register          # 회원가입
POST   /api/auth/login             # 로그인
POST   /api/auth/refresh           # 토큰 갱신
POST   /api/auth/logout            # 로그아웃
GET    /api/auth/me                # 현재 사용자 정보
```

### 게시글 (Posts)

```
GET    /api/posts                  # 글 목록 (페이지네이션, 필터)
GET    /api/posts/{id}             # 글 상세 조회
GET    /api/posts/slug/{slug}      # Slug로 조회
POST   /api/posts                  # 글 작성 (인증 필요)
PUT    /api/posts/{id}             # 글 수정 (작성자/관리자)
DELETE /api/posts/{id}             # 글 삭제 (작성자/관리자)
GET    /api/posts/{id}/revisions   # 글 히스토리
POST   /api/posts/{id}/publish     # 글 발행
GET    /api/posts/search           # 전문 검색
```

### 카테고리 (Categories)

```
GET    /api/categories             # 카테고리 목록
GET    /api/categories/{id}        # 카테고리 상세
POST   /api/categories             # 카테고리 생성 (관리자)
PUT    /api/categories/{id}        # 카테고리 수정 (관리자)
DELETE /api/categories/{id}        # 카테고리 삭제 (관리자)
```

### 태그 (Tags)

```
GET    /api/tags                   # 태그 목록
GET    /api/tags/{id}              # 태그 상세
POST   /api/tags                   # 태그 생성 (관리자)
DELETE /api/tags/{id}              # 태그 삭제 (관리자)
```

### 댓글 (Comments)

```
GET    /api/posts/{postId}/comments      # 댓글 목록
POST   /api/posts/{postId}/comments      # 댓글 작성
PUT    /api/comments/{id}                # 댓글 수정
DELETE /api/comments/{id}                # 댓글 삭제
```

### 미디어 (Media)

```
POST   /api/media/upload           # 이미지 업로드
GET    /api/media/{id}             # 이미지 정보 조회
DELETE /api/media/{id}             # 이미지 삭제
```

### SEO

```
GET    /sitemap.xml                # 사이트맵
GET    /robots.txt                 # robots.txt
GET    /api/posts/{id}/metadata    # 메타데이터 (OG 태그)
```

### Request/Response 예시

#### POST /api/posts (글 작성)

**Request**:
```json
{
  "title": "ASP.NET Core 성능 최적화",
  "content": "# 소개\n\n성능 최적화 방법...",
  "summary": "ASP.NET Core 애플리케이션의 성능을 향상시키는 방법",
  "categoryId": 1,
  "tagIds": [1, 3, 5],
  "status": "Draft",
  "thumbnailUrl": "/uploads/thumbnail.jpg",
  "seoKeywords": "ASP.NET, 성능, 최적화",
  "metaDescription": "ASP.NET Core 성능 최적화 가이드"
}
```

**Response**:
```json
{
  "id": 42,
  "title": "ASP.NET Core 성능 최적화",
  "slug": "aspnet-core-performance-optimization",
  "content": "# 소개\n\n성능 최적화 방법...",
  "summary": "ASP.NET Core 애플리케이션의 성능을 향상시키는 방법",
  "userId": 1,
  "categoryId": 1,
  "tags": [
    { "id": 1, "name": "ASP.NET", "slug": "aspnet" },
    { "id": 3, "name": "성능", "slug": "performance" }
  ],
  "status": "Draft",
  "viewCount": 0,
  "readingTimeMinutes": 8,
  "createdAt": "2025-11-04T10:30:00Z",
  "updatedAt": "2025-11-04T10:30:00Z"
}
```

#### GET /api/posts (글 목록 with 필터링)

**Query Parameters**:
```
?page=1
&pageSize=10
&status=Published
&categoryId=1
&tagId=3
&search=ASP.NET
&sortBy=createdAt
&sortOrder=desc
```

**Response**:
```json
{
  "items": [
    {
      "id": 42,
      "title": "ASP.NET Core 성능 최적화",
      "slug": "aspnet-core-performance-optimization",
      "summary": "ASP.NET Core 애플리케이션의 성능을 향상시키는 방법",
      "thumbnailUrl": "/uploads/thumbnail.jpg",
      "viewCount": 1523,
      "readingTimeMinutes": 8,
      "createdAt": "2025-11-04T10:30:00Z",
      "author": {
        "id": 1,
        "username": "jpk",
        "avatarUrl": "/uploads/avatar.jpg"
      },
      "category": {
        "id": 1,
        "name": "Backend",
        "slug": "backend"
      },
      "tags": [
        { "id": 1, "name": "ASP.NET" }
      ]
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalItems": 48
  }
}
```

---

## 보안 전략

### 1. 인증 & 인가 (JWT)

```csharp
// JWT 설정
{
  "AccessToken": {
    "ExpiresIn": "15m",
    "Issuer": "BlogAPI",
    "Audience": "BlogClient"
  },
  "RefreshToken": {
    "ExpiresIn": "7d"
  }
}
```

- **Access Token**: 15분 (짧은 수명)
- **Refresh Token**: 7일 (HttpOnly Cookie 저장)
- **토큰 저장**: HttpOnly + Secure + SameSite=Strict

### 2. Rate Limiting

```csharp
// 로그인 시도: 5회/분
// 회원가입: 3회/시간
// API 일반: 100회/분
// 이미지 업로드: 10회/시간
```

### 3. XSS 방어

- Content Security Policy (CSP)
- HTML 이스케이핑
- DOMPurify (프론트엔드)

### 4. CSRF 방어

- SameSite Cookie
- CSRF Token (폼 제출 시)

### 5. SQL Injection 방어

- Parameterized Queries (EF Core 자동 처리)
- Input Validation (FluentValidation)

### 6. 파일 업로드 검증

```csharp
// 허용 MIME 타입: image/jpeg, image/png, image/webp
// 최대 파일 크기: 5MB
// 파일명 검증: 특수문자 제거
// 이미지 검증: ImageSharp로 실제 이미지 확인
```

### 7. HTTPS

- Let's Encrypt 인증서
- HTTP → HTTPS 리디렉션
- HSTS 헤더

---

## 성능 최적화

### 1. 데이터베이스 최적화

#### 인덱스 전략
```sql
-- 복합 인덱스 (자주 함께 조회되는 컬럼)
CREATE INDEX idx_posts_status_created ON posts(status, created_at DESC);

-- Full-Text Search
CREATE INDEX idx_posts_search ON posts USING GIN (to_tsvector('english', title || ' ' || content));
```

#### 쿼리 최적화
```csharp
// N+1 문제 방지: Include 사용
var posts = await _context.Posts
    .Include(p => p.User)
    .Include(p => p.Category)
    .Include(p => p.PostTags)
        .ThenInclude(pt => pt.Tag)
    .Where(p => p.Status == PostStatus.Published)
    .OrderByDescending(p => p.PublishedAt)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### 2. 캐싱 전략 (Redis - Phase 2)

```yaml
Cache Strategy:
  - 글 목록: 5분 TTL
  - 인기 글: 1시간 TTL
  - 카테고리/태그: 1일 TTL
  - 사용자 정보: 30분 TTL
  - 조회수: Redis Counter (1분마다 DB 동기화)
```

### 3. 조회수 최적화

```
사용자 조회 → Redis INCR → 백그라운드 작업 (1분마다) → DB 업데이트
```

### 4. 이미지 최적화

- WebP 자동 변환
- 썸네일 생성 (여러 크기)
- Lazy Loading (next/image)
- CDN 연동 (추후)

### 5. API 응답 최적화

- Pagination (기본값: 10개)
- 필드 선택 (필요한 데이터만)
- Compression (Gzip/Brotli)

---

## 배포 전략

### Docker Compose 구성

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: blog_db
      POSTGRES_USER: blog_user
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./backups:/backups
    ports:
      - "5432:5432"
    restart: unless-stopped

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    restart: unless-stopped

  backend:
    build:
      context: ./Blog.API
      dockerfile: Dockerfile
    environment:
      ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=blog_db;Username=blog_user;Password=${DB_PASSWORD}"
      ConnectionStrings__Redis: "redis:6379"
      JWT_SECRET: ${JWT_SECRET}
      ASPNETCORE_ENVIRONMENT: Production
    ports:
      - "5000:5000"
    depends_on:
      - postgres
      - redis
    restart: unless-stopped

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    environment:
      NEXT_PUBLIC_API_URL: ${API_URL}
    ports:
      - "3000:3000"
    depends_on:
      - backend
    restart: unless-stopped

  nginx:
    image: nginx:alpine
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf
      - ./nginx/ssl:/etc/nginx/ssl
    ports:
      - "80:80"
      - "443:443"
    depends_on:
      - frontend
      - backend
    restart: unless-stopped

volumes:
  postgres_data:
```

### CI/CD (GitHub Actions)

```yaml
# .github/workflows/deploy.yml
name: Deploy Blog Platform

on:
  push:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run Tests
        run: dotnet test

  build-and-deploy:
    needs: test
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Build Docker Images
        run: docker-compose build

      - name: Deploy to Home Server
        uses: appleboy/ssh-action@master
        with:
          host: ${{ secrets.SERVER_HOST }}
          username: ${{ secrets.SERVER_USER }}
          key: ${{ secrets.SERVER_SSH_KEY }}
          script: |
            cd /opt/blog
            git pull origin main
            docker-compose pull
            docker-compose up -d
            docker system prune -af
```

### 백업 전략

```bash
# 매일 자정 DB 백업
0 0 * * * docker exec blog_postgres pg_dump -U blog_user blog_db > /backups/db_$(date +\%Y\%m\%d).sql

# 7일 이상 된 백업 삭제
0 1 * * * find /backups -name "*.sql" -mtime +7 -delete

# 이미지 파일 주간 백업 (rsync)
0 2 * * 0 rsync -av /opt/blog/uploads /backups/images
```

---

## 개발 로드맵

### 📍 Phase 1: Core MVP (2-3주)
**목표**: 기본 블로그 기능

- [ ] Entity 모델 정의 (User, Post, Category, Tag)
- [ ] ApplicationDbContext 구성
- [ ] 마이그레이션 생성
- [ ] JWT 인증 구현
- [ ] Post CRUD API
- [ ] Category/Tag API
- [ ] Swagger 문서화
- [ ] 기본 에러 핸들링

### 📍 Phase 2: 성능 & UX (2주)
**목표**: 성능 최적화 및 사용자 경험 개선

- [ ] Redis 캐싱
- [ ] 조회수 비동기 업데이트
- [ ] 댓글 시스템 (대댓글)
- [ ] 이미지 업로드
- [ ] Full-Text Search
- [ ] Rate Limiting
- [ ] 초안 자동 저장

### 📍 Phase 3: 고급 기능 (2-3주)
**목표**: 블로그 플랫폼 완성

- [ ] 글 버전 히스토리
- [ ] 북마크
- [ ] 예약 발행
- [ ] 추천글
- [ ] Sitemap.xml
- [ ] Open Graph 메타데이터
- [ ] 통계 API

### 📍 Phase 4: 운영 (1-2주)
**목표**: 프로덕션 배포 준비

- [ ] GitHub Actions CI/CD
- [ ] 자동 백업
- [ ] Health Check
- [ ] 모니터링 (선택)
- [ ] 단위 테스트
- [ ] 보안 강화

---

## 다음 단계

### 즉시 시작 가능한 작업

1. **Entity 모델 생성**
   - User, Category, Tag, Comment, MediaFile 등

2. **ApplicationDbContext 완성**
   - DbSet 등록
   - 관계 설정

3. **마이그레이션 생성 및 적용**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **기본 Controller 생성**
   - PostsController, AuthController

5. **JWT 인증 설정**
   - JwtTokenGenerator 유틸리티

---

## 참고 자료

- [ASP.NET Core 공식 문서](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [PostgreSQL Full-Text Search](https://www.postgresql.org/docs/current/textsearch.html)
- [Next.js 공식 문서](https://nextjs.org/docs)
- [SEO Best Practices](https://developers.google.com/search/docs)

---

**문서 버전**: 2.0
**최종 수정**: 2025-11-04
**작성자**: Architecture Team