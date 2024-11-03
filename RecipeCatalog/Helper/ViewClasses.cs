using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeCatalog.Helper
{
    public class MissingViewRightGroupItem
    {
        public int ID;
        [Translation("Attribute_GroupName")]
        public string GroupName { get; set; }
        [Translation("Attribute_CannotAccess")]
        public bool CannotAccess { get; set; }
    }

    public class MissingViewRightComponentItem
    {
        public int ID;
        [Translation("Attribute_ComponentName")]
        public string ComponentName { get; set; }
        [Translation("Attribute_CannotSee")]
        public bool CannotSee { get; set; }
        [Translation("Attribute_CannotSeeDescription")]
        public bool CannotSeeDescription { get; set; }
    }

    public class MissingViewRightComponentUserItem
    {
        public Guid UserID;
        [Translation("Attribute_UserName")]
        public string UserName { get; set; }
        [Translation("Attribute_CannotSee")]
        public bool CannotSee { get; set; }
        [Translation("Attribute_CannotSeeDescription")]
        public bool CannotSeeDescription { get; set; }
    }

    public class MissingViewRightRecipeItem
    {
        public int ID;
        [Translation("Attribute_RecipeName")]
        public string RecipeName { get; set; }
        [Translation("Attribute_CannotSee")]
        public bool CannotSee { get; set; }
        [Translation("Attribute_CannotSeeDescription")]
        public bool CannotSeeDescription { get; set; }
        [Translation("Attribute_CannotSeeComponents")]
        public bool CannotSeeComponents { get; set; }
    }

    public class MissingViewRightRecipeUserItem
    {
        public Guid UserID;
        [Translation("Attribute_UserName")]
        public string UserName { get; set; }
        [Translation("Attribute_CannotSee")]
        public bool CannotSee { get; set; }
        [Translation("Attribute_CannotSeeDescription")]
        public bool CannotSeeDescription { get; set; }
        [Translation("Attribute_CannotSeeComponents")]
        public bool CannotSeeComponents { get; set; }
    }
}
