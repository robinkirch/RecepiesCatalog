using CommunityToolkit.Maui.Views;
using RecipeCatalog.Models;
using RecipeCatalog.Resources.Language;
using System.Collections.ObjectModel;

namespace RecipeCatalog.Popups;

public partial class AddRecipePopup : Popup
{
    private byte[] selectedImage;
    public AddRecipePopup()
	{
		InitializeComponent();
        LoadPickerData();
        LoadComponents();
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

    private async void OnSelectImageClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = AppLanguage.Select_Picture,
            FileTypes = FilePickerFileType.Images
        });

        if (result != null)
        {
            var stream = await result.OpenReadAsync();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                selectedImage = memoryStream.ToArray();
            }
        }
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
            Image = selectedImage,
            Name = NameEntry.Text,
            Description = DescriptionEntry.Text,
            Aliases = (AliasesEntry.Text != null) ? AliasesEntry.Text.Split(',') : [],
            GroupId = (GroupPicker.SelectedIndex != -1) ? ((Group)GroupPicker.SelectedItem).Id : null,
            Components = RecipesComponents

        });
        MauiProgram._context.SaveChanges();
        Close(MauiProgram._context.Recipes.Single(c => c.Name == NameEntry.Text));
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}