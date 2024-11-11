using CommunityToolkit.Maui.Views;
using RecipeCatalog.Helper;
using RecipeCatalog.Models;
using RecipeCatalog.Resources.Language;
using System.Collections.ObjectModel;

namespace RecipeCatalog.Popups;

public partial class AddRecipePopup : Popup
{
    private readonly User _user;

    public AddRecipePopup(User user)
	{
        _user = user;
        InitializeComponent();
        LoadPickerData();
        LoadData();
        LoadCollectionViews();
    }

    /// <summary>
    /// Loads user data for the recipe creation process.
    /// Retrieves the user's view rights and populates the associated collection views for visibility and description settings.
    /// </summary>
    private void LoadData()
    {
        var userItems = new ObservableCollection<MissingViewRightRecipeUserItem>();
        MauiProgram._context.Users.ToList().ForEach(u =>
        {
            userItems.Add(new MissingViewRightRecipeUserItem { UserID = u.Id, UserName = u.Username, CannotSee = false, CannotSeeDescription = false, CannotSeeComponents = false });
        });
        DynamicTableControlRights.ItemsSource = new ObservableCollection<object>(userItems.Cast<object>());
        DynamicTableControlRights.BuildTable(AppLanguage.User_CustomRights);
    }

    /// <summary>
    /// Loads the components available for the recipe.
    /// Retrieves all components from the database and populates the component collection view.
    /// </summary>
    private void LoadCollectionViews()
    {
        var componentItem = new ObservableCollection<ComponentItem>();
        MauiProgram._context.Components.OrderBy(c => c.Name).ToList().ForEach(c =>
        {
            componentItem.Add(new ComponentItem { ID = c.Id, ComponentName = c.Name, Quantity = 0  });
        });
        DynamicTableControlQuantityComponents.ItemsSource = new ObservableCollection<object>(componentItem.Cast<object>());
        DynamicTableControlQuantityComponents.BuildTable(AppLanguage.Filter_Components);

        var recipeItem = new ObservableCollection<RecipeItem>();
        MauiProgram._context.Recipes.OrderBy(c => c.Name).ToList().ForEach(c =>
        {
            recipeItem.Add(new RecipeItem { ID = c.Id, RecipeName = c.Name, Quantity = 0 });
        });
        DynamicTableControlQuantityRecipes.ItemsSource = new ObservableCollection<object>(recipeItem.Cast<object>());
        DynamicTableControlQuantityRecipes.BuildTable(AppLanguage.Filter_Recipes);
    }

    /// <summary>
    /// Loads data for the group picker.
    /// Populates the group picker with available groups from the database.
    /// </summary>
    private void LoadPickerData()
    {
        CategoryPicker.ItemsSource = MauiProgram._context.Categories.ToList();
        CategoryPicker.ItemDisplayBinding = new Binding(nameof(Category.CategoryName));
    }

    /// <summary>
    /// Handles the event when the "Send" button is clicked.
    /// Creates a new recipe, associates selected components, and saves the recipe along with user view rights in the database.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Event data.</param>
    private void OnSendButtonClicked(object sender, EventArgs e)
    {
        var newRecipe = MauiProgram._context.Recipes.Add(new Recipe
        {
            Image = null, // set in detailpage
            Name = NameEntry.Text,
            Description = DescriptionEntry.Text,
            SecretDescription = SecretDescriptionEntry.Text,
            Aliases = (AliasesEntry.Text != null) ? AliasesEntry.Text.Split(',') : [],
            CategoryId = (CategoryPicker.SelectedIndex != -1) ? ((Category)CategoryPicker.SelectedItem).Id : null,
        });
        MauiProgram._context.SaveChanges();


        DynamicTableControlQuantityComponents.ItemsSource.ToList().ForEach(item =>
        {
            if (item is ComponentItem c && c.Quantity != 0) // Pattern Matching is neccessary
                MauiProgram._context.RecipeComponents.Add(new() { RecipeId = newRecipe.Entity.Id, Count = c.Quantity, ComponentId = c.ID });
        });
        MauiProgram._context.SaveChanges();

        DynamicTableControlQuantityRecipes.ItemsSource.ToList().ForEach(item =>
        {
            if (item is RecipeItem c && c.Quantity != 0) // Pattern Matching is neccessary
                MauiProgram._context.RecipeComponents.Add(new() { RecipeId = newRecipe.Entity.Id, Count = c.Quantity, UsedRecipeId = c.ID });
        });
        MauiProgram._context.SaveChanges();


        DynamicTableControlRights.ItemsSource.ToList().ForEach(item =>
        {
            if (item is MissingViewRightRecipeUserItem c && (c.CannotSee || c.CannotSeeDescription || c.CannotSeeComponents)) // Pattern Matching is neccessary
                MauiProgram._context.MissingViewRightsRecipes.Add(new() { RecipeId = newRecipe.Entity.Id, UserId = c.UserID, CannotSee = c.CannotSee, CannotSeeDescription = c.CannotSeeDescription, CannotSeeComponents = c.CannotSeeComponents });
        });
        MauiProgram._context.SaveChanges();
        //TODO: error display
        Close(MauiProgram._context.Recipes.Single(c => c.Name == NameEntry.Text));
    }

    /// <summary>
    /// Handles the event when the "Cancel" button is clicked.
    /// Closes the popup without saving any changes.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Event data.</param>
    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}