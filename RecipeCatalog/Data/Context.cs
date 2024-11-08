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
        public DbSet<User> Users { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<MissingViewRightComponent> MissingViewRightsComponents { get; set; }
        public DbSet<MissingViewRightRecipe> MissingViewRightsRecipes { get; set; }
        public DbSet<MissingViewRightGroup> MissingViewRightsGroups { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if(_connectionString != null && _connectionString != string.Empty)
                    optionsBuilder.UseMySql(_connectionString, new MySqlServerVersion(new Version(10, 3, 39)));
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

                entity.HasMany(r => r.Recipes)
                    .WithOne(rc => rc.UsedRecipeNavigation)
                    .HasForeignKey(rc => rc.UsedRecipeId);

                entity.HasOne(r => r.GroupNavigation)
                    .WithMany(g => g.Recipes)
                    .HasForeignKey(r => r.GroupId);
            });

            modelBuilder.Entity<RecipeComponents>(entity =>
            {
                entity.HasKey(rc => rc.Id);

                entity.Property(rc => rc.Count)
                    .IsRequired();

                entity.HasOne(rc => rc.RecipeNavigation)
                    .WithMany(r => r.Components)
                    .HasForeignKey(rc => rc.RecipeId);

                entity.HasOne(rc => rc.ComponentNavigation)
                    .WithMany(c => c.RecipeComponents)
                    .HasForeignKey(rc => rc.ComponentId);

                entity.HasOne(rc => rc.UsedRecipeNavigation)
                    .WithMany(c => c.Recipes)
                    .HasForeignKey(rc => rc.UsedRecipeId);
            });

            modelBuilder.Entity<MissingViewRightGroup>(entity =>
            {
                entity.HasKey(mvr => mvr.Id);

                entity.HasOne(mvr => mvr.User)
                    .WithMany(u => u.MissingViewRightsGroup)
                    .HasForeignKey(mvr => mvr.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mvr => mvr.Group)
                    .WithMany(g => g.MissingViewRightsGroup)
                    .HasForeignKey(mvr => mvr.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MissingViewRightComponent>(entity =>
            {
                entity.HasKey(mvr => mvr.Id);

                entity.HasOne(mvr => mvr.User)
                    .WithMany(u => u.MissingViewRightsComponent)
                    .HasForeignKey(mvr => mvr.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mvr => mvr.Component)
                    .WithMany(g => g.MissingViewRightsComponent)
                    .HasForeignKey(mvr => mvr.ComponentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MissingViewRightRecipe>(entity =>
            {
                entity.HasKey(mvr => mvr.Id);

                entity.HasOne(mvr => mvr.User)
                    .WithMany(u => u.MissingViewRightsRecipe)
                    .HasForeignKey(mvr => mvr.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mvr => mvr.Recipe)
                    .WithMany(g => g.MissingViewRightsRecipes)
                    .HasForeignKey(mvr => mvr.RecipeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.HasOne(b => b.User)
                    .WithMany(u => u.Bookmarks)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Group)
                    .WithMany(g => g.Bookmarks)
                    .HasForeignKey(b => b.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Component)
                    .WithMany()
                    .HasForeignKey(b => b.ComponentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(b => b.Recipe)
                    .WithMany()
                    .HasForeignKey(b => b.RecipeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Username)
                      .IsRequired()
                      .HasMaxLength(255)
                      .IsUnicode(false);

                entity.HasOne(u => u.CampaignNavigation)
                      .WithMany(c => c.Users)
                      .HasForeignKey(u => u.CampaignId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}