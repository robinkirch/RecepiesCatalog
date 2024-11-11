using System.ComponentModel.DataAnnotations;

namespace RecipeCatalog.Models
{
    public class Category
    {
        public Category()
        {
            Components = new HashSet<Component>();
            Recipes = new HashSet<Recipe>();
            MissingViewRightsCategories = new HashSet<MissingViewRightCategory>();
            Bookmarks = new HashSet<Bookmark>();
        }

        [Key]
        public int Id { get; set; }
        public string CategoryName { get; set; }

        public virtual ICollection<Component> Components { get; set; }
        public virtual ICollection<Recipe> Recipes { get; set; }
        public virtual ICollection<MissingViewRightCategory> MissingViewRightsCategories { get; set; }
        public virtual ICollection<Bookmark> Bookmarks { get; set; }
    }
}
