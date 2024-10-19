using RecipeCatalog.Data;
using System.ComponentModel.DataAnnotations;

namespace RecipeCatalog.Models
{
    public class Recipe : IData
    {
        public Recipe()
        {
            Components = new HashSet<RecipeComponents>();
            MissingViewRightsRecipes = new HashSet<MissingViewRightRecipe>();
        }

        [Key]
        public int Id { get; set; }
        public byte[]? Image { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string[]? Aliases { get; set; }
        public int? GroupId { get; set; }

        public virtual Group? GroupNavigation { get; set; }
        public virtual ICollection<RecipeComponents> Components { get; set; }
        public virtual ICollection<MissingViewRightRecipe> MissingViewRightsRecipes { get; set; }
    }

    public class RecipeComponents
    {
        [Key]
        public int Id { get; set; }
        public int Count { get; set; }
        public int? ComponentId { get; set; }
        public int? RecipeId { get; set; }

        public virtual Recipe? RecipeNavigation { get; set; } 
        public virtual Component? ComponentNavigation { get; set; }
    }
}