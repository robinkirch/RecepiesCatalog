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
        LoadComponents();
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
    private void LoadComponents()
    {
        var components = new ObservableCollection<ComponentView>();
        MauiProgram._context.Components.ToList().ForEach(c =>
        {
            components.Add(new() { Name = c.Name, Id = c.Id, IsSelected = false, Count = 0 });
        });
        ComponentCollectionView.ItemsSource = components;
    }

    /// <summary>
    /// Loads data for the group picker.
    /// Populates the group picker with available groups from the database.
    /// </summary>
    private void LoadPickerData()
    {
        GroupPicker.ItemsSource = MauiProgram._context.Groups.ToList();
        GroupPicker.ItemDisplayBinding = new Binding("GroupName");
    }

    /// <summary>
    /// Handles the event when the "Send" button is clicked.
    /// Creates a new recipe, associates selected components, and saves the recipe along with user view rights in the database.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Event data.</param>
    private void OnSendButtonClicked(object sender, EventArgs e)
    {
        var RecipesComponents = new List<RecipeComponents>();
        foreach (ComponentView item in ComponentCollectionView.ItemsSource)
        {
            if (item.IsSelected)
            {
                RecipesComponents.Add(new() { Count = item.Count, ComponentId = item.Id});
            }
        }
        MauiProgram._context.RecipeComponents.AddRange(RecipesComponents);
        MauiProgram._context.SaveChanges();

        var newRecipe = MauiProgram._context.Recipes.Add(new Recipe
        {
            Image = null, // set in detailpage
            Name = NameEntry.Text,
            Description = DescriptionEntry.Text,
            SecretDescription = SecretDescriptionEntry.Text,
            Aliases = (AliasesEntry.Text != null) ? AliasesEntry.Text.Split(',') : [],
            GroupId = (GroupPicker.SelectedIndex != -1) ? ((Group)GroupPicker.SelectedItem).Id : null,
            Components = RecipesComponents

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