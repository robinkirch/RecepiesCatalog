using System.ComponentModel.DataAnnotations;

namespace RecipeCatalog.Models
{
    //You can basically see everything unless you have an entry for a category that you cannot see.
    public class MissingViewRightCategory
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int CategoryId { get; set; }

        public virtual User User { get; set; }
        public virtual Category Category { get; set; }
    }
}
