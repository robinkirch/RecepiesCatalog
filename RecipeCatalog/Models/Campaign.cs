using System.ComponentModel.DataAnnotations;

namespace RecipeCatalog.Models
{
    public class Campaign
    {
        public Campaign()
        {
            Users = new HashSet<User>();
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
