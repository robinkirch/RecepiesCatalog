using Microsoft.EntityFrameworkCore;
using RecipeCatalog.Models;

namespace RecipeCatalog.Data
{
    public class Context : DbContext
    {
        private readonly string _connectionString;

        public Context(string connectionString)
        {
            _connectionString = connectionString;
        }
        DbSet<Component> Components { get; set; }
        DbSet<Recipe> Recepies { get; set; }
        DbSet<RecipeComponents> RecipeComponents { get; set; }
        DbSet<Group> Groups { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Definieren der Primärschlüssel
            modelBuilder.Entity<Recipe>().HasKey(r => r.Id);
            modelBuilder.Entity<Component>().HasKey(c => c.Id);
            modelBuilder.Entity<RecipeComponents>().HasKey(rc => rc.Id);
            modelBuilder.Entity<Group>().HasKey(g => g.Id);

            // Konfigurieren der Relationen
            modelBuilder.Entity<RecipeComponents>()
                .HasOne(rc => rc.Component)
                .WithMany()  // Da keine explizite Rückreferenz definiert ist
                .HasForeignKey(rc => rc.Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecipeComponents>()
                .HasOne<Recipe>()
                .WithMany(r => r.Components)
                .HasForeignKey(rc => rc.Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Component>()
                .HasOne(c => c.Group)
                .WithMany()  // Falls Group viele Components haben kann
                .HasForeignKey(c => c.Id);

            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Group)
                .WithMany()  // Falls Group viele Recipes haben kann
                .HasForeignKey(r => r.Id);

            // Konvertieren der Aliases als JSON oder CSV
            modelBuilder.Entity<Recipe>()
                .Property(r => r.Aliases)
                .HasConversion(
                    v => string.Join(',', v), // Konvertiert Array zu CSV
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)); // Konvertiert CSV zu Array

            modelBuilder.Entity<Component>()
                .Property(c => c.Aliases)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
        }


    }
}
