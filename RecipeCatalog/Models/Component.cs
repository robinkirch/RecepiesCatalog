using System.ComponentModel.DataAnnotations;

namespace RecipeCatalog.Models
{
    internal class Component
    {
        [Key]
        public int Id { get; set; }
        public byte[]? Image {  get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string[]? Aliases { get; set; }
        public Group? Group { get; set; }
    }
}
