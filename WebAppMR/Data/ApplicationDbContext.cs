using Microsoft.EntityFrameworkCore;
using WebAppMR.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<User> Users { get; set; } = null!; // Ensure that the Users DbSet is initialized
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure your entity mappings here
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
        });
    }
}