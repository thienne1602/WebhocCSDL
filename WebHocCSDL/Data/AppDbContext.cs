using Microsoft.EntityFrameworkCore;
using WebHocCSDL.Models;

namespace WebHocCSDL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<DatabaseDesign> Designs { get; set; }
        public DbSet<Entity> Entities { get; set; }
        public DbSet<Relationship> Relationships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DatabaseDesign>()
                .Property(d => d.RequirementDescription)
                .HasMaxLength(5000);

            modelBuilder.Entity<DatabaseDesign>()
                .Property(d => d.ERD)
                .HasMaxLength(5000);

            modelBuilder.Entity<DatabaseDesign>()
                .Property(d => d.LogicalDesign)
                .HasMaxLength(5000);

            modelBuilder.Entity<DatabaseDesign>()
                .Property(d => d.PhysicalDesign)
                .HasMaxLength(5000);

            modelBuilder.Entity<DatabaseDesign>()
                .Property(d => d.ConceptualDesign)
                .HasMaxLength(5000);

            modelBuilder.Entity<DatabaseDesign>()
                .HasMany(d => d.Entities)
                .WithOne(e => e.DatabaseDesign)
                .HasForeignKey(e => e.DatabaseDesignId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DatabaseDesign>()
                .HasMany(d => d.Relationships)
                .WithOne(r => r.DatabaseDesign)
                .HasForeignKey(r => r.DatabaseDesignId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Entity>()
                .Property(e => e.Name)
                .HasMaxLength(255);

            modelBuilder.Entity<Entity>()
                .Property(e => e.Attributes)
                .HasConversion(
                    v => Newtonsoft.Json.JsonConvert.SerializeObject(v ?? new List<string>()),
                    v => Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>()
                );

            modelBuilder.Entity<Relationship>()
                .Property(r => r.Entity1)
                .HasMaxLength(255);

            modelBuilder.Entity<Relationship>()
                .Property(r => r.Entity2)
                .HasMaxLength(255);

            modelBuilder.Entity<Relationship>()
                .Property(r => r.Type)
                .HasMaxLength(10);
        }
    }
}