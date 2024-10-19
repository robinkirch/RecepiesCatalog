using System.ComponentModel.DataAnnotations;

namespace RecipeCatalog.Models
{
    //You can basically see everything unless you have an entry for a group that you cannot see.
    public class MissingViewRightGroup
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int GroupId { get; set; }

        public virtual User User { get; set; }
        public virtual Group Group { get; set; }
    }
}
