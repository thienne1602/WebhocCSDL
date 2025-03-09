using Microsoft.EntityFrameworkCore;
using WebHocCSDL.Models;

namespace WebHocCSDL.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<DatabaseDesign> Designs { get; set; }
        public DbSet<Exercise> Exercises { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DatabaseDesign>().HasKey(d => d.Id);
            modelBuilder.Entity<Exercise>().HasKey(e => e.Id);
        }
    }
}