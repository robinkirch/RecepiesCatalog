using System.ComponentModel.DataAnnotations;

namespace RecipeCatalog.Models
{
    internal class Recipe
    {
        [Key]
        public int Id { get; set; }
        public byte[]? Image { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string[]? Aliases { get; set; }
        public List<RecipeComponents>? Components { get; set; } = new();
        public Group? Group { get; set; }
    }

    internal class RecipeComponents
    {
        [Key]
        public int Id { get; set; }
        public int Count { get; set; }
        public Component Component { get; set; }
    }

    internal class Group
    {
        [Key]
        public int Id { get; set; }
        public int GroupName { get; set; }
    }
}