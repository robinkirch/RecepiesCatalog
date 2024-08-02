using System.ComponentModel.DataAnnotations;

namespace RecipeCatalog.Models
{
    public class Component
    {
        public Component()
        {
            RecipeComponents = new HashSet<RecipeComponents>();
        }

        [Key]
        public int Id { get; set; }
        public byte[]? Image {  get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string[]? Aliases { get; set; }
        public int? GroupId { get; set; }

        public virtual Group? GroupNavigation { get; set; }
        public virtual ICollection<RecipeComponents> RecipeComponents { get; set; }
    }
}
