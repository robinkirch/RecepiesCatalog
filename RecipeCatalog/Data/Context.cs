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
        public DbSet<Component> Components { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeComponents> RecipeComponents { get; set; }
        public DbSet<Group> Groups { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
                //optionsBuilder.UseMySql(_connectionString, new MySqlServerVersion(new Version(10, 3, 39)));
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(g => g.Id);

                entity.Property(g => g.GroupName)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasMany(g => g.Components)
                    .WithOne(c => c.GroupNavigation)
                    .HasForeignKey(c => c.GroupId);

                entity.HasMany(g => g.Recipes)
                    .WithOne(r => r.GroupNavigation)
                    .HasForeignKey(r => r.GroupId);
            });

            modelBuilder.Entity<Component>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasMany(c => c.RecipeComponents)
                    .WithOne(rc => rc.ComponentNavigation)
                    .HasForeignKey(rc => rc.ComponentId);
            });

            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Name)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasMany(r => r.Components)
                    .WithOne(rc => rc.RecipeNavigation)
                    .HasForeignKey(rc => rc.RecipeId);

                entity.HasOne(r => r.GroupNavigation)
                    .WithMany(g => g.Recipes)
                    .HasForeignKey(r => r.GroupId);
            });

            modelBuilder.Entity<RecipeComponents>(entity =>
            {
                entity.HasKey(rc => rc.Id);

                entity.Property(rc => rc.Count)
                    .IsRequired();

                entity.Property(rc => rc.ComponentId)
                    .IsRequired();

                entity.HasOne(rc => rc.RecipeNavigation)
                    .WithMany(r => r.Components)
                    .HasForeignKey(rc => rc.RecipeId);

                entity.HasOne(rc => rc.ComponentNavigation)
                    .WithMany(c => c.RecipeComponents)
                    .HasForeignKey(rc => rc.ComponentId);
            });
        }
    }
}