using Blog.Data;
using Blog.Services.Implementations;
using Blog.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Controllers 추가 (API 엔드포인트 사용을 위해 필수)
builder.Services.AddControllers();

// DbContext 등록 (데이터베이스 연결)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// PostService 등록 (Dependency Injection)
// Scoped: HTTP 요청당 한 번만 인스턴스 생성
builder.Services.AddScoped<IPostService, PostService>();

var app = builder.Build();

// 개발 환경에서 초기 데이터 시딩
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbSeeder.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Controllers 라우팅 활성화 (API 엔드포인트를 실제로 등록)
app.MapControllers();

// DB 상태 확인용 테스트 엔드포인트
app.MapGet("/db-status", async (ApplicationDbContext context) =>
{
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

        return Results.Ok(new
        {
            Connected = canConnect,
            Database = context.Database.GetDbConnection().Database,
            Server = context.Database.GetDbConnection().DataSource,
            PendingMigrations = pendingMigrations.ToList(),
            AppliedMigrations = appliedMigrations.Count(),
            Status = canConnect ? "Healthy" : "Unhealthy"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();