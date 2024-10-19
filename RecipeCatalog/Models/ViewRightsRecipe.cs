using System.ComponentModel.DataAnnotations;

namespace RecipeCatalog.Models
{
    public class MissingViewRightRecipe
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int RecipeId { get; set; }
        public bool CannotSee { get; set; }
        public bool CannotSeeDescription { get; set; }
        public bool CannotSeeComponents { get; set; }

        public virtual User User { get; set; }
        public virtual Recipe Recipe { get; set; }
    }
}
