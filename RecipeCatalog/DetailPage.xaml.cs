using Microsoft.Maui.Controls;
using RecipeCatalog.Data;
using RecipeCatalog.Models;

namespace RecipeCatalog;

public partial class DetailPage : ContentPage
{
	public DetailPage(Component data)
	{
		InitializeComponent();
        ChangeCommonData(data);
        HR.IsVisible = false;
        AddUsedIn(data.Id);
    }

    public DetailPage(Recipe data)
    {
        InitializeComponent();
        ChangeCommonData(data);
        AddComponents(MauiProgram._context.RecipeComponents.Where(rc => rc.RecipeId == data.Id).ToList());
    }

    public void ChangeCommonData(IData data)
    {
        DetailImage.Source = MauiProgram.ByteArrayToImageSource(data.Image);
        AliasText.Text = (data.Aliases != null && data.Aliases.Length > 0) ? "(" + string.Join(",", data.Aliases) + ")" : string.Empty;
        NameText.Text = data.Name;
        DescriptionText.Text = data.Description;
        GroupText.Text = MauiProgram._context.Groups.Single(g => g.Id == data.GroupId).GroupName;
    }

    public void AddUsedIn(int componentId)
    {
        var recipes = MauiProgram._context.RecipeComponents.Where(rc => rc.ComponentId == componentId)
            .Select(rc => MauiProgram._context.Recipes.Single(r => r.Id == rc.RecipeId)).ToList();

        if (recipes.Count > 0)
        {
            HR.IsVisible = true;
            Recipes.IsVisible = true;
            int row = 0;

            recipes.ForEach(r =>
            {
                RecipesComponentsPlace.RowDefinitions.Add(new RowDefinition { Height = 20 });
                //TODO: Link to detailpage
                var nameLabel = new Label
                {
                    Text = r.Name,
                    VerticalOptions = LayoutOptions.Start
                };
                RecipesComponentsPlace.Children.Add(nameLabel);
                Grid.SetRow(nameLabel, row);
                Grid.SetColumn(nameLabel, 0);
                row++;
            });
        }
    }

    public void AddComponents(List<RecipeComponents> data)
    {
        int row = 0;
        if (data.Count != 0)
        {
            Components.IsVisible = true;

            //head
            var componentLabel = new Label
            {
                Text = "Components",
                FontAttributes = FontAttributes.Bold,
                FontSize = 15,
                VerticalOptions = LayoutOptions.Start
            };
            RecipesComponentsPlace.Children.Add(componentLabel);
            Grid.SetRow(componentLabel, row);
            Grid.SetColumn(componentLabel, 0);
            var counterLabel = new Label
            {
                Text = "Count",
                FontAttributes = FontAttributes.Bold,
                FontSize = 15,
                VerticalOptions = LayoutOptions.Start
            };
            RecipesComponentsPlace.Children.Add(counterLabel);
            Grid.SetRow(counterLabel, row);
            Grid.SetColumn(counterLabel, 2);

            row++;
        }

        foreach (var item in data)
        {
            RecipesComponentsPlace.RowDefinitions.Add(new RowDefinition { Height = 20 });
            //TODO: Link to detailpage
            var nameLabel = new Label
            {
                Text = MauiProgram._context.Components.Where(c => c.Id == item.ComponentId).Select(c => c.Name).Single(),
                VerticalOptions = LayoutOptions.Start
            };
            RecipesComponentsPlace.Children.Add(nameLabel);
            Grid.SetRow(nameLabel, row);
            Grid.SetColumn(nameLabel, 0);

            var countLabel = new Label
            {
                
                Text = item.Count.ToString(),
                VerticalOptions = LayoutOptions.Center
            };
            RecipesComponentsPlace.Children.Add(countLabel);
            Grid.SetRow(countLabel, row);
            Grid.SetColumn(countLabel, 2);

            row++;
        }
    }

    private async void OnBack(object sender, EventArgs e)
    {
        //TODO: Missing searchdata
        App.Current.MainPage = new SearchAndViewPage();
    }

    private async void OnEdit(object sender, EventArgs e)
    {
        //TODO: Popup
    }

    private async void OnDelete(object sender, EventArgs e)
    {
        //TODO: Popup
    }
}