using System.ComponentModel.DataAnnotations;

namespace RecipeCatalog.Models
{
    public class Bookmark
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int? ComponentId { get; set; }
        public int? RecipeId { get; set; }
        public int GroupId { get; set; }

        public virtual User User { get; set; }
        public virtual Group Group { get; set; }
        public virtual Component? Component { get; set; }
        public virtual Recipe? Recipe { get; set; }
    }
}
