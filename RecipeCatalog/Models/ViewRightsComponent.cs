using System.ComponentModel.DataAnnotations;

namespace RecipeCatalog.Models
{
    public class MissingViewRightComponent
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int ComponentId { get; set; }
        public bool CannotSee { get; set; }
        public bool CannotSeeDescription { get; set; }

        public virtual User User { get; set; }
        public virtual Component Component { get; set; }
    }
}
