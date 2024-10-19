using System.ComponentModel.DataAnnotations;

namespace RecipeCatalog.Models
{
    public class Group
    {
        public Group()
        {
            Components = new HashSet<Component>();
            Recipes = new HashSet<Recipe>();
            MissingViewRightsGroup = new HashSet<MissingViewRightGroup>();
            Bookmarks = new HashSet<Bookmark>();
        }

        [Key]
        public int Id { get; set; }
        public string GroupName { get; set; }

        public virtual ICollection<Component> Components { get; set; }
        public virtual ICollection<Recipe> Recipes { get; set; }
        public virtual ICollection<MissingViewRightGroup> MissingViewRightsGroup { get; set; }
        public virtual ICollection<Bookmark> Bookmarks { get; set; }
    }
}
