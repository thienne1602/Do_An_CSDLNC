using Microsoft.EntityFrameworkCore;
using WebCSDL.Models;

namespace WebCSDL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Entity> Entities { get; set; }
        public DbSet<EntityAttribute> EntityAttributes { get; set; }
        public DbSet<Relationship> Relationships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>()
                .Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            modelBuilder.Entity<EntityAttribute>()
                .HasOne(a => a.Entity)
                .WithMany(e => e.Attributes)
                .HasForeignKey(a => a.EntityId);

            modelBuilder.Entity<Relationship>()
                .HasKey(r => new { r.Entity1, r.Entity2 }); // Khóa chính ghép
        }
    }
}