using System.ComponentModel.DataAnnotations;

namespace RecipeCatalog.Models
{
    public class User
    {
        public User()
        {
            MissingViewRightsComponent = new HashSet<MissingViewRightComponent>();
            MissingViewRightsRecipe = new HashSet<MissingViewRightRecipe>();
            MissingViewRightsCategory = new HashSet<MissingViewRightCategory>();
            Bookmarks = new HashSet<Bookmark>();
        }

        [Key]
        public Guid Id { get; set; }
        public string Username { get; set; }
        public bool IsAdmin { get; set; }
        public int? CampaignId { get; set; }

        public virtual Campaign? CampaignNavigation { get; set; }
        public virtual ICollection<MissingViewRightComponent> MissingViewRightsComponent { get; set; }
        public virtual ICollection<MissingViewRightRecipe> MissingViewRightsRecipe { get; set; }
        public virtual ICollection<MissingViewRightCategory> MissingViewRightsCategory { get; set; }
        public virtual ICollection<Bookmark> Bookmarks { get; set; }
    }

    public class UserView()
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public bool IsSelected { get; set; }
    }
}
