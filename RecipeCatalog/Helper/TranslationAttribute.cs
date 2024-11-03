namespace RecipeCatalog.Helper
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TranslationAttribute : Attribute
    {
        public string TranslationKey { get; }
        public TranslationAttribute(string translationKey)
        {
            TranslationKey = translationKey;
        }
    }
}