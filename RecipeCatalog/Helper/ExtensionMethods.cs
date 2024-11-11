using RecipeCatalog.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RecipeCatalog.Helper
{
    internal static class ExtensionMethods
    {
        public static List<IData> SearchFilter(this IEnumerable<IData> data, string searchtext)
            => data.Where(c =>
            c.Name.ToLower().Contains(searchtext.ToLower()) ||
            (c.Description != null && c.Description.ToLower().Contains(searchtext.ToLower())) ||
            (c.Aliases != null && c.Aliases.Length > 0 && c.Aliases[0].Length > 0 && string.Join(",", c.Aliases).ToLower().Contains(searchtext.ToLower()))).ToList();
    }
}