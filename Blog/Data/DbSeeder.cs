using Blog.Model;

namespace Blog.Data;

/// <summary>
/// 데이터베이스 초기 데이터 시딩 클래스
/// 개발 환경에서 테스트용 데이터를 자동으로 생성합니다.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// 데이터베이스에 초기 데이터를 추가합니다.
    /// Program.cs에서 앱 시작 시 한 번 호출됩니다.
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // 이미 데이터가 있으면 스킵
        if (context.Users.Any())
        {
            return;
        }

        // 테스트용 사용자 추가
        var admin = new User
        {
            Email = "admin@blog.com",
            PasswordHash = "temp_hash_123", // TODO: 실제로는 BCrypt 해시 사용
            Username = "admin",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(admin);

        // 테스트용 카테고리 추가
        var techCategory = new Category
        {
            Name = "기술",
            Slug = "tech",
            Description = ".NET, C#, 데이터베이스 등 기술 관련 포스트",
            DisplayOrder = 1,
            CreatedAt = DateTime.UtcNow
        };

        var dailyCategory = new Category
        {
            Name = "일상",
            Slug = "daily",
            Description = "일상적인 이야기와 생각들",
            DisplayOrder = 2,
            CreatedAt = DateTime.UtcNow
        };

        context.Categories.AddRange(techCategory, dailyCategory);

        // 테스트용 태그 추가
        var csharpTag = new Tag
        {
            Name = "C#",
            Slug = "csharp",
            CreatedAt = DateTime.UtcNow
        };

        var dotnetTag = new Tag
        {
            Name = ".NET",
            Slug = "dotnet",
            CreatedAt = DateTime.UtcNow
        };

        var postgresTag = new Tag
        {
            Name = "PostgreSQL",
            Slug = "postgresql",
            CreatedAt = DateTime.UtcNow
        };

        context.Tags.AddRange(csharpTag, dotnetTag, postgresTag);

        // 변경사항 저장
        await context.SaveChangesAsync();

        Console.WriteLine("✅ 초기 데이터 시딩 완료!");
    }
}
