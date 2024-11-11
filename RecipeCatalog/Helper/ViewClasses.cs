namespace RecipeCatalog.Helper
{
    public class MissingViewRightCategorieItem
    {
        public int ID;
        [Translation("Attribute_CategoryName")]
        public string CategoryName { get; set; }
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

    public class ComponentItem
    {
        public int ID;
        [Translation("Attribute_ComponentName")]
        public string ComponentName { get; set; }
        [Translation("Attribute_Quantity")]
        public int Quantity { get; set; }
    }

    public class RecipeItem
    {
        public int ID;
        [Translation("Attribute_RecipeName")]
        public string RecipeName { get; set; }
        [Translation("Attribute_Quantity")]
        public int Quantity { get; set; }
    }
}
