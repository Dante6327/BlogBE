# 🗺️ 블로그 플랫폼 개발 로드맵

> **작성일**: 2025-11-04
> **예상 기간**: 8-10주
> **현재 상태**: Phase 1 진행 중

---

## 📋 목차

1. [개발 원칙](#개발-원칙)
2. [Phase 1: Core MVP](#phase-1-core-mvp-2-3주)
3. [Phase 2: 성능 최적화 & UX](#phase-2-성능-최적화--ux-개선-2주)
4. [Phase 3: 고급 기능](#phase-3-고급-기능-2-3주)
5. [Phase 4: 운영 환경 구축](#phase-4-운영-환경-구축-1-2주)
6. [진행 상황 트래킹](#진행-상황-트래킹)

---

## 개발 원칙

### 1. 점진적 개발
- 각 Phase가 독립적으로 배포 가능
- Phase 1 완료 후 바로 사용 가능한 MVP
- 기능 추가가 기존 코드를 깨뜨리지 않음

### 2. 테스트 우선
- 핵심 비즈니스 로직에 단위 테스트 필수
- API 엔드포인트 통합 테스트
- CI/CD 파이프라인에서 자동 테스트

### 3. 문서화
- 코드 주석 (복잡한 로직)
- API 문서 (Swagger)
- README 업데이트

### 4. 코드 리뷰
- PR 단위 개발
- 셀프 리뷰 후 머지

---

## Phase 1: Core MVP (2-3주)

**목표**: 기본 블로그 기능 구현 - 글 작성/읽기 가능

### 🎯 완료 기준
- [ ] 사용자 회원가입/로그인 가능
- [ ] 글 작성/수정/삭제 가능
- [ ] 글 목록 조회 (페이지네이션)
- [ ] 글 상세 조회
- [ ] 카테고리/태그 관리
- [ ] Swagger API 문서 확인 가능

### Week 1: 백엔드 기초 구축

#### Day 1-2: 데이터 모델 및 DB 설정
- [x] ~~프로젝트 초기화~~
- [x] ~~PostgreSQL 연결 설정~~
- [ ] Entity 모델 생성
  - [ ] `User.cs`
  - [ ] `Post.cs`
  - [ ] `Category.cs`
  - [ ] `Tag.cs`
  - [ ] `PostTag.cs`
- [ ] `ApplicationDbContext` 완성
  - [ ] DbSet 등록
  - [ ] OnModelCreating 관계 설정
- [ ] 초기 마이그레이션 생성 및 적용
  ```bash
  dotnet ef migrations add InitialCreate
  dotnet ef database update
  ```

#### Day 3-4: 인증 구현
- [ ] DTOs 생성
  - [ ] `RegisterRequest.cs`
  - [ ] `LoginRequest.cs`
  - [ ] `AuthResponse.cs`
- [ ] `IAuthService` 인터페이스
- [ ] `AuthService` 구현
  - [ ] 회원가입 로직
  - [ ] 비밀번호 해싱 (BCrypt)
  - [ ] JWT 토큰 생성
- [ ] `JwtTokenGenerator` 유틸리티
- [ ] `AuthController` 생성
  - [ ] `POST /api/auth/register`
  - [ ] `POST /api/auth/login`
- [ ] JWT 미들웨어 설정 (`Program.cs`)
- [ ] 테스트 (Postman/Swagger)

#### Day 5-7: 게시글 CRUD API
- [ ] DTOs 생성
  - [ ] `CreatePostRequest.cs`
  - [ ] `UpdatePostRequest.cs`
  - [ ] `PostResponse.cs`
  - [ ] `PostListResponse.cs`
- [ ] `IPostService` 인터페이스
- [ ] `PostService` 구현
  - [ ] 글 생성 (Slug 자동 생성)
  - [ ] 글 조회 (단일/목록)
  - [ ] 글 수정 (작성자 확인)
  - [ ] 글 삭제 (Soft Delete)
  - [ ] 페이지네이션
- [ ] `SlugGenerator` 유틸리티
- [ ] `PostsController` 생성
  - [ ] `GET /api/posts` (목록)
  - [ ] `GET /api/posts/{id}` (상세)
  - [ ] `POST /api/posts` (작성, 인증 필요)
  - [ ] `PUT /api/posts/{id}` (수정)
  - [ ] `DELETE /api/posts/{id}` (삭제)
- [ ] 권한 체크 (`[Authorize]` 속성)

### Week 2: 카테고리/태그 & 에러 핸들링

#### Day 8-9: 카테고리 & 태그 API
- [ ] `Category` & `Tag` DTOs
- [ ] `CategoryService` & `TagService`
- [ ] `CategoriesController`
  - [ ] `GET /api/categories`
  - [ ] `POST /api/categories` (관리자만)
  - [ ] `PUT /api/categories/{id}`
  - [ ] `DELETE /api/categories/{id}`
- [ ] `TagsController`
  - [ ] `GET /api/tags`
  - [ ] `POST /api/tags` (관리자만)
  - [ ] `DELETE /api/tags/{id}`
- [ ] 글 작성 시 태그 연결 기능

#### Day 10-11: 에러 핸들링 & 로깅
- [ ] `ErrorHandlingMiddleware` 생성
  - [ ] 전역 예외 처리
  - [ ] 표준화된 에러 응답
- [ ] Serilog 설정
  - [ ] 콘솔 로그
  - [ ] 파일 로그 (일별 롤링)
  - [ ] 로그 레벨 설정
- [ ] FluentValidation 통합
  - [ ] `CreatePostValidator`
  - [ ] `RegisterUserValidator`
- [ ] Custom Exceptions
  - [ ] `NotFoundException`
  - [ ] `UnauthorizedException`

#### Day 12-14: Swagger & 통합 테스트
- [ ] Swagger 설정 개선
  - [ ] JWT 인증 지원
  - [ ] API 설명 추가
  - [ ] 예시 Request/Response
- [ ] API 통합 테스트 작성
  - [ ] `AuthControllerTests`
  - [ ] `PostsControllerTests`
- [ ] Postman 컬렉션 생성

### Week 3: 프론트엔드 기초 (선택)

**Note**: 백엔드 우선 개발이므로, Phase 1에서는 프론트엔드를 건너뛰고 Phase 2 이후 진행 가능

- [ ] Next.js 프로젝트 초기화
- [ ] Tailwind CSS 설정
- [ ] 기본 레이아웃
- [ ] 로그인/회원가입 페이지
- [ ] 글 목록 페이지
- [ ] 글 상세 페이지

### Phase 1 완료 체크리스트

- [ ] 사용자 회원가입/로그인 동작
- [ ] JWT 인증 적용
- [ ] 글 CRUD 동작
- [ ] 카테고리/태그 관리 동작
- [ ] Swagger 문서 확인
- [ ] 에러 핸들링 동작
- [ ] 로그 파일 생성 확인
- [ ] 기본 단위 테스트 통과

---

## Phase 2: 성능 최적화 & UX 개선 (2주)

**목표**: 성능 향상 및 사용자 경험 개선

### 🎯 완료 기준
- [ ] Redis 캐싱 적용
- [ ] 조회수 비동기 업데이트
- [ ] 댓글 시스템 동작
- [ ] 이미지 업로드 가능
- [ ] 전문 검색 동작
- [ ] Rate Limiting 적용

### Week 4: Redis & 캐싱

#### Day 15-17: Redis 통합
- [ ] Docker Compose에 Redis 추가
- [ ] `IDistributedCache` 설정
- [ ] 캐싱 전략 구현
  - [ ] 글 목록 캐시 (5분 TTL)
  - [ ] 인기 글 캐시 (1시간 TTL)
  - [ ] 카테고리/태그 캐시 (1일 TTL)
- [ ] 캐시 무효화 로직
  - [ ] 글 생성/수정/삭제 시
  - [ ] 카테고리/태그 변경 시

#### Day 18-19: 조회수 최적화
- [ ] Redis Counter 구현
  - [ ] `POST /api/posts/{id}/view` 엔드포인트
  - [ ] Redis INCR 사용
- [ ] 백그라운드 작업 (Hosted Service)
  - [ ] 1분마다 Redis → DB 동기화
  - [ ] 배치 업데이트 쿼리
- [ ] 중복 조회 방지 (IP/Session 기반)

#### Day 20-21: 검색 기능
- [ ] PostgreSQL Full-Text Search
  - [ ] GIN 인덱스 생성 (마이그레이션)
  - [ ] `to_tsvector`, `to_tsquery` 사용
- [ ] 검색 API
  - [ ] `GET /api/posts/search?q=keyword`
  - [ ] 제목 + 본문 검색
  - [ ] 결과 하이라이팅 (선택)

### Week 5: 댓글 & 이미지 업로드

#### Day 22-24: 댓글 시스템
- [ ] `Comment` 엔티티 마이그레이션
- [ ] 댓글 DTOs
  - [ ] `CreateCommentRequest`
  - [ ] `CommentResponse`
- [ ] `CommentService` 구현
  - [ ] 댓글 작성 (대댓글 지원)
  - [ ] 댓글 수정/삭제
  - [ ] 계층 구조 조회
- [ ] `CommentsController`
  - [ ] `GET /api/posts/{postId}/comments`
  - [ ] `POST /api/posts/{postId}/comments`
  - [ ] `PUT /api/comments/{id}`
  - [ ] `DELETE /api/comments/{id}`

#### Day 25-28: 이미지 업로드
- [ ] `MediaFile` 엔티티 마이그레이션
- [ ] 파일 업로드 설정
  - [ ] 최대 크기: 5MB
  - [ ] 허용 형식: jpg, png, webp
- [ ] `MediaService` 구현
  - [ ] 파일 검증 (MIME 타입, 크기)
  - [ ] 파일명 Sanitize
  - [ ] 로컬 저장 (`/uploads`)
  - [ ] 썸네일 생성 (ImageSharp)
- [ ] `MediaController`
  - [ ] `POST /api/media/upload`
  - [ ] `GET /api/media/{id}`
  - [ ] `DELETE /api/media/{id}`
- [ ] Rate Limiting (10회/시간)

### Phase 2 완료 체크리스트

- [ ] Redis 연결 확인
- [ ] 캐시된 데이터 조회 동작
- [ ] 조회수 증가 동작
- [ ] 댓글 작성/대댓글 동작
- [ ] 이미지 업로드 동작
- [ ] 전문 검색 결과 확인
- [ ] Rate Limiting 동작 확인

---

## Phase 3: 고급 기능 (2-3주)

**목표**: 블로그 플랫폼 완성도 향상

### 🎯 완료 기준
- [ ] 글 버전 히스토리
- [ ] 북마크 기능
- [ ] 예약 발행
- [ ] 추천글 설정
- [ ] SEO 최적화 완료
- [ ] 통계 API

### Week 6-7: 고급 기능 구현

#### Day 29-31: 글 버전 히스토리
- [ ] `PostRevision` 엔티티 마이그레이션
- [ ] 글 수정 시 자동 히스토리 생성
- [ ] `GET /api/posts/{id}/revisions` API
- [ ] 특정 버전으로 복원 기능 (선택)

#### Day 32-34: 북마크 & 추천글
- [ ] `Bookmark` 엔티티 마이그레이션
- [ ] 북마크 API
  - [ ] `POST /api/posts/{id}/bookmark`
  - [ ] `DELETE /api/posts/{id}/bookmark`
  - [ ] `GET /api/users/me/bookmarks`
- [ ] 추천글 기능
  - [ ] `is_featured` 플래그
  - [ ] `GET /api/posts/featured`

#### Day 35-37: 예약 발행
- [ ] `PostStatus.Scheduled` 상태 추가
- [ ] 예약 발행 설정
  - [ ] `published_at` 미래 시간 설정
- [ ] 백그라운드 작업 (Hosted Service)
  - [ ] 1분마다 스케줄 확인
  - [ ] 시간 도래 시 자동 발행

#### Day 38-42: SEO 최적화
- [ ] Sitemap.xml 생성
  - [ ] `GET /sitemap.xml`
  - [ ] 발행된 글 목록
  - [ ] 주간 업데이트
- [ ] robots.txt
  - [ ] `GET /robots.txt`
- [ ] Open Graph 메타데이터 API
  - [ ] `GET /api/posts/{id}/metadata`
  - [ ] OG 태그 정보 반환
- [ ] 읽기 시간 자동 계산
  - [ ] 글 저장 시 자동 계산 (평균 200단어/분)

### Week 8: 통계 & 분석

#### Day 43-45: 통계 API
- [ ] 대시보드 통계
  - [ ] `GET /api/stats/dashboard`
  - [ ] 총 글 수, 총 조회수, 총 댓글 수
- [ ] 인기 글
  - [ ] `GET /api/posts/popular` (조회수 기준)
- [ ] 최근 글
  - [ ] `GET /api/posts/recent`
- [ ] 카테고리별 글 수
  - [ ] `GET /api/categories/{id}/stats`

### Phase 3 완료 체크리스트

- [ ] 글 수정 히스토리 확인
- [ ] 북마크 추가/제거 동작
- [ ] 예약 발행 자동 동작
- [ ] Sitemap.xml 생성 확인
- [ ] OG 메타데이터 출력
- [ ] 통계 API 데이터 확인

---

## Phase 4: 운영 환경 구축 (1-2주)

**목표**: 프로덕션 배포 및 운영 안정화

### 🎯 완료 기준
- [ ] Docker Compose 완성
- [ ] GitHub Actions CI/CD 동작
- [ ] 자동 백업 설정
- [ ] Health Check 동작
- [ ] 단위 테스트 통과

### Week 9-10: DevOps & 테스트

#### Day 46-48: Docker & CI/CD
- [ ] Dockerfile 작성
  - [ ] Backend (ASP.NET Core)
  - [ ] Frontend (Next.js)
- [ ] docker-compose.yml 완성
  - [ ] PostgreSQL
  - [ ] Redis
  - [ ] Backend
  - [ ] Frontend
  - [ ] Nginx
- [ ] .env 환경 변수 관리
- [ ] GitHub Actions 워크플로우
  - [ ] 테스트 실행
  - [ ] Docker 빌드
  - [ ] 홈서버 배포

#### Day 49-51: 백업 & 모니터링
- [ ] 자동 DB 백업 스크립트
  - [ ] 매일 자정 실행 (Cron)
  - [ ] 7일 이상 된 백업 삭제
- [ ] 이미지 파일 백업
  - [ ] 주간 백업 (rsync)
- [ ] Health Check 엔드포인트
  - [ ] `GET /health`
  - [ ] DB 연결 확인
  - [ ] Redis 연결 확인
- [ ] Prometheus + Grafana (선택)
  - [ ] Metrics 수집
  - [ ] 대시보드 생성

#### Day 52-56: 테스트 & 보안
- [ ] 단위 테스트 작성
  - [ ] `PostServiceTests`
  - [ ] `AuthServiceTests`
  - [ ] `CommentServiceTests`
- [ ] 통합 테스트 작성
  - [ ] API 엔드포인트 테스트
- [ ] 보안 강화
  - [ ] HTTPS 설정 (Let's Encrypt)
  - [ ] CORS 정책 확인
  - [ ] Rate Limiting 확인
  - [ ] SQL Injection 테스트
  - [ ] XSS 방어 확인

### Phase 4 완료 체크리스트

- [ ] Docker Compose 실행 성공
- [ ] GitHub Actions 배포 성공
- [ ] 자동 백업 파일 생성 확인
- [ ] Health Check 응답 확인
- [ ] 단위 테스트 모두 통과
- [ ] HTTPS 연결 동작
- [ ] Rate Limiting 동작

---

## 진행 상황 트래킹

### 전체 진행률

```
Phase 1: Core MVP              [████████░░] 80% (현재)
Phase 2: 성능 최적화 & UX      [░░░░░░░░░░]  0%
Phase 3: 고급 기능             [░░░░░░░░░░]  0%
Phase 4: 운영 환경 구축        [░░░░░░░░░░]  0%

전체 진행률: 20%
```

### 현재 작업 중

- [x] 프로젝트 초기화
- [x] PostgreSQL 연결
- [ ] Entity 모델 정의 ← **현재 위치**
- [ ] 마이그레이션 생성

### 다음 작업

1. User, Post, Category, Tag, PostTag 엔티티 생성
2. ApplicationDbContext 완성 (DbSet 등록, 관계 설정)
3. 마이그레이션 생성 및 적용
4. AuthService 구현

---

## 마일스톤

| 마일스톤 | 예상 완료일 | 상태 |
|---------|------------|------|
| Phase 1 완료 | 2025-11-25 | 🔄 진행 중 |
| Phase 2 완료 | 2025-12-09 | ⏳ 대기 중 |
| Phase 3 완료 | 2025-12-30 | ⏳ 대기 중 |
| Phase 4 완료 | 2026-01-13 | ⏳ 대기 중 |
| **프로덕션 배포** | **2026-01-20** | ⏳ 대기 중 |

---

## 리스크 관리

| 리스크 | 발생 가능성 | 영향 | 완화 전략 |
|--------|------------|------|-----------|
| PostgreSQL 성능 이슈 | 중간 | 높음 | 인덱스 최적화, 쿼리 튜닝 |
| 홈서버 다운타임 | 낮음 | 높음 | Health Check, 자동 재시작 |
| 보안 취약점 | 중간 | 높음 | 정기 보안 감사, OWASP 체크리스트 |
| 일정 지연 | 높음 | 중간 | Phase별 독립 배포, MVP 우선 |

---

## 성공 지표 (KPI)

### Phase 1 (MVP)
- [ ] API 응답 시간 < 500ms
- [ ] Swagger 문서 100% 완성
- [ ] 코드 커버리지 > 60%

### Phase 2 (최적화)
- [ ] API 응답 시간 < 200ms (캐시 적용)
- [ ] 조회수 업데이트 지연 < 1분
- [ ] 이미지 업로드 성공률 > 99%

### Phase 3 (고급 기능)
- [ ] SEO 점수 > 90 (Lighthouse)
- [ ] Sitemap.xml 자동 생성
- [ ] 예약 발행 정확도 100%

### Phase 4 (운영)
- [ ] 배포 자동화 100%
- [ ] 백업 성공률 100%
- [ ] Uptime > 99.5%

---

## 참고 자료

- [ASP.NET Core Best Practices](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)
- [Entity Framework Core Performance](https://docs.microsoft.com/en-us/ef/core/performance/)
- [PostgreSQL Performance Tips](https://wiki.postgresql.org/wiki/Performance_Optimization)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)

---

**문서 버전**: 1.0
**최종 수정**: 2025-11-04
**다음 리뷰**: 매주 월요일