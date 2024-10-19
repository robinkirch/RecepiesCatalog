using RecipeCatalog.Data;
using System.ComponentModel.DataAnnotations;

namespace RecipeCatalog.Models
{
    public class Component : IData
    {
        public Component()
        {
            RecipeComponents = new HashSet<RecipeComponents>();
            MissingViewRightsComponent = new HashSet<MissingViewRightComponent>();
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
        public virtual ICollection<MissingViewRightComponent> MissingViewRightsComponent { get; set; }
    }

    public class ComponentView()
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public int Count { get; set; }
    }
}
