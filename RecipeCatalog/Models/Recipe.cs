namespace RecipeCatalog.Models
{
    internal class Recipe
    {
        public byte[]? Image { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string[]? Aliases { get; set; }
        public List<RecipeComponents>? Components { get; set; } = new();
    }

    internal class RecipeComponents
    {
        public int Count { get; set; }
        public Component Component { get; set; }
    }
}