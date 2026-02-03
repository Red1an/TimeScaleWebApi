using Microsoft.EntityFrameworkCore;
using TimeScaleWebApi.Models;

namespace TimeScaleWebApi.Data
{
    public class ApplicationContext : DbContext
    {

        public DbSet<TimeValues> Values { get; set; } = null!;
        public DbSet<TimeResults> Results { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"Переменная окружения DB_CONNECTION не задана {connectionString}.");
            }
            optionsBuilder.UseNpgsql(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TimeResults>(entity =>
            {
                entity.HasKey(tr => tr.Id);
                entity.HasIndex(tr => tr.Filename)
                .IsUnique();
            });



            modelBuilder.Entity<TimeValues>(entity =>
            {
                entity.HasKey(tv => tv.Id);
                entity.HasIndex(tv => new { tv.Filename, tv.Date });
            });
                
        }
    }
}
