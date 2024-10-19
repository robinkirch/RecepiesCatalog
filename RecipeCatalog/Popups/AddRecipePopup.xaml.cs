using CommunityToolkit.Maui.Views;
using RecipeCatalog.Models;
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

    private void LoadData()
    {
        var userRights = MauiProgram._context.MissingViewRightsRecipes.Where(m => m.UserId == _user.Id).ToList();
        var viewSettings = new ObservableCollection<UserView>();
        var compSettings = new ObservableCollection<UserView>();
        var descSettings = new ObservableCollection<UserView>();
        MauiProgram._context.Users.ToList().ForEach(u =>
        {
            viewSettings.Add(new() { UserName = u.Username, Id = u.Id, IsSelected = false });
            descSettings.Add(new() { UserName = u.Username, Id = u.Id, IsSelected = false });
            compSettings.Add(new() { UserName = u.Username, Id = u.Id, IsSelected = false });
        });
        ViewCollectionView.ItemsSource = viewSettings;
        DescCollectionView.ItemsSource = descSettings;
        CompCollectionView.ItemsSource = compSettings;
    }

    private void LoadComponents()
    {
        var components = new ObservableCollection<ComponentView>();
        MauiProgram._context.Components.ToList().ForEach(c =>
        {
            components.Add(new() { Name = c.Name, Id = c.Id, IsSelected = false, Count = 0 });
        });
        ComponentCollectionView.ItemsSource = components;
    }
    private void LoadPickerData()
    {
        GroupPicker.ItemsSource = MauiProgram._context.Groups.ToList();
        GroupPicker.ItemDisplayBinding = new Binding("GroupName");
    }

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

        var newRecipes = MauiProgram._context.Recipes.Add(new Recipe
        {
            Image = null, // set in detailpage
            Name = NameEntry.Text,
            Description = DescriptionEntry.Text,
            Aliases = (AliasesEntry.Text != null) ? AliasesEntry.Text.Split(',') : [],
            GroupId = (GroupPicker.SelectedIndex != -1) ? ((Group)GroupPicker.SelectedItem).Id : null,
            Components = RecipesComponents

        });
        MauiProgram._context.SaveChanges();

        List<UserView> descs = (DescCollectionView.ItemsSource as ObservableCollection<UserView>)!.ToList();
        List<UserView> comps = (CompCollectionView.ItemsSource as ObservableCollection<UserView>)!.ToList();
        (ViewCollectionView.ItemsSource as ObservableCollection<UserView>)!.ToList().ForEach(u =>
        {
            bool descSelected = descs.Where(d => d.Id == u.Id).Single().IsSelected;
            bool compSelected = comps.Where(d => d.Id == u.Id).Single().IsSelected;
            var entry = MauiProgram._context.MissingViewRightsRecipes.Where(m => m.UserId == u.Id && m.RecipeId == newRecipes.Entity.Id).SingleOrDefault();
            if (entry == null && (u.IsSelected || descSelected || compSelected))
            {
                entry = new()
                {
                    UserId = u.Id,
                    RecipeId = newRecipes.Entity.Id,
                    CannotSee = u.IsSelected,
                    CannotSeeDescription = descSelected,
                    CannotSeeComponents = compSelected,
                };
                MauiProgram._context.MissingViewRightsRecipes.Add(entry);
            }
            else if (entry != null)
            {
                entry.CannotSee = u.IsSelected;
                entry.CannotSeeDescription = descSelected;
                entry.CannotSeeComponents = compSelected;
            }
            MauiProgram._context.SaveChanges();
        });

        Close(MauiProgram._context.Recipes.Single(c => c.Name == NameEntry.Text));
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}