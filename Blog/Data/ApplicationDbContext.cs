using Microsoft.EntityFrameworkCore;

namespace Blog.Data;

public class ApplicationDbContext :DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    //public DbSet<EntityName> TableName { get; set; }
}