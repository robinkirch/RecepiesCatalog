namespace RecipeCatalog.Models
{
    internal class Component
    {
        public byte[]? Image {  get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string[]? Aliases { get; set; }
    }
}
