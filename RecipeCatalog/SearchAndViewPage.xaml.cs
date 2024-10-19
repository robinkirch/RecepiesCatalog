using CommunityToolkit.Maui.Views;
using RecipeCatalog.Data;
using RecipeCatalog.Models;
using RecipeCatalog.Popups;
using RecipeCatalog.Resources.Language;

namespace RecipeCatalog;

public partial class SearchAndViewPage : ContentPage
{
    private readonly bool _isInitialized = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchAndViewPage"/> class.
    /// Sets up the search and view page, loading necessary data based on the user's permissions and previous selections.
    /// </summary>
    /// <param name="results">Optional. A list of <see cref="IData"/> objects (components and recipes) to be displayed. If null, retrieves viewable data for the current user.</param>
    /// <param name="search">Optional. The search text to pre-fill the search entry.</param>
    /// <param name="selectedIndex">Optional. The index of the selected group type in the picker.</param>
    public SearchAndViewPage(List<IData>? results = null, string search = "", int selectedIndex = 0)
	{
		InitializeComponent();
        _isInitialized = false;
        if(MauiProgram._context.Users.Where(u => u.Id == Guid.Parse(MauiProgram.Configuration.GetSection("Connection:UserKey").Value!)).Select(u => u.IsAdmin).Single())
            AdminArea.IsVisible = true;

        LoadPickerData(search, selectedIndex);
        LoadView(results);
        _isInitialized = true;
    }

    /// <summary>
    /// Loads data into the picker component, including pre-filling the search entry and setting the selected index.
    /// Retrieves available groups and populates the picker with them.
    /// </summary>
    /// <param name="search">The search text to be displayed in the search entry.</param>
    /// <param name="selectedIndex">The index of the selected group in the picker.</param>
    private void LoadPickerData(string search, int selectedIndex)
    {
        SearchEntry.Text = search;

        List<Group> entrys = new List<Group>() { new() { GroupName = "-" }, new() { GroupName = AppLanguage.Filter_Components }, new() { GroupName = AppLanguage.Filter_Recipes } };
        entrys.AddRange(MauiProgram._context.Groups.ToList());
        TypeGroupPicker.ItemsSource = entrys;
        TypeGroupPicker.ItemDisplayBinding = new Binding("GroupName");
        TypeGroupPicker.SelectedIndex = selectedIndex;
    }

    /// <summary>
    /// Loads viewable components and recipes into a grid layout based on the user's view rights.
    /// If no results are provided, it retrieves all of the viewable data for the current user.
    /// Filters the content by view rights for components, recipes, and groups, then displays them in a grid with images, names, and descriptions.
    /// </summary>
    /// <param name="results">Optional. A list of <see cref="IData"/> objects (components and recipes) to be displayed. If null, retrieves viewable data for the current user.</param>
    private void LoadView(List<IData>? results = null)
    {
        var missingRightsComponent = MauiProgram._context.MissingViewRightsComponents.Where(m => m.UserId == MauiProgram.CurrentUser.Id).ToList();
        var missingRightsRecipe = MauiProgram._context.MissingViewRightsRecipes.Where(m => m.UserId == MauiProgram.CurrentUser.Id).ToList();
        var missingRightsGroup = MauiProgram._context.MissingViewRightsGroups.Where(m => m.UserId == MauiProgram.CurrentUser.Id).ToList();

        results ??= RetrieveViewableDataForUser();

        int numberOfColumns = 2;
        for (int i = 0; i < numberOfColumns; i++)
        {
            ResultView.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        }

        for (int i = 0; i < (results.Count + numberOfColumns - 1) / numberOfColumns; i++)
        {
            ResultView.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        for (int i = 0; i < results.Count; i++)
        {
            var currentObject = results[i];
            bool canAccessGroup = !missingRightsGroup.Any(g => g.GroupId == results[i].GroupId);
            string denied = "-";
            var frame = new Frame
            {
                BorderColor = canAccessGroup ? Color.Parse("Gray") : Color.Parse("DarkRed"),
                CornerRadius = 5,
                Padding = 10,
                HasShadow = true,
                BackgroundColor = Color.FromArgb("#50D3D3D3"),
                Content = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    Children =
                    {
                        new Frame
                        {
                            CornerRadius = 10,
                            BackgroundColor = Color.FromHsla(0, 0, 0, 0) ,
                            HeightRequest = 100,
                            WidthRequest = 100,
                            BorderColor = Color.FromHsla(0, 0, 0, 0) ,
                            Content = new Image
                            {
                                Source = MauiProgram.ByteArrayToImageSource(currentObject.Image),
                                Aspect = Aspect.AspectFill,
                                WidthRequest = 100,
                                HeightRequest = 100
                            }
                        },
                        new Label
                        {
                            Text = currentObject.Name,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 16
                        },
                        new Label
                        {
                            Text = (canAccessGroup) ? currentObject switch
                            {
                                Component component => !missingRightsComponent.Any(c => c.ComponentId == component.Id && c.CannotSeeDescription)
                                    ? GetTruncatedDescription(currentObject.Description)
                                    : denied,

                                Recipe recipe => !missingRightsRecipe.Any(r => r.RecipeId == recipe.Id && r.CannotSeeDescription)
                                    ? GetTruncatedDescription(currentObject.Description)
                                    : denied,

                                _ => denied
                            } : denied,
                            FontSize = 14
                        },
                        new Label
                        {
                            Text = currentObject.GroupId != null ? MauiProgram._context.Groups.Where(g => g.Id == currentObject.GroupId).Select(g => g.GroupName).Single() : string.Empty,
                            FontSize = 9,
                            TextColor = canAccessGroup ? Color.Parse("DarkGray") : Color.Parse("DarkRed"),
                        }
                    }
                }
            };
            if (canAccessGroup)
            {
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) => OnFrameTapped(currentObject.Id, currentObject.GetType());
                frame.GestureRecognizers.Add(tapGestureRecognizer);
            }

            ResultView.Children.Add(frame);
            Grid.SetRow(frame, i / numberOfColumns);
            Grid.SetColumn(frame, i % numberOfColumns);
        }
    }

    /// <summary>
    /// Truncates the description text to the specified length and adds ellipsis if the text exceeds the limit.
    /// </summary>
    /// <param name="desc">The description text to be truncated.</param>
    /// <param name="length">Optional. The maximum length of the truncated description. Defaults to 200 characters.</param>
    /// <returns>A truncated description with ellipsis if it exceeds the specified length, or the full description if it's within the limit.</returns>
    private string GetTruncatedDescription(string? desc, int length = 200) => desc != null && desc.Length > length ? desc.Substring(0, length) + "..." : desc ?? string.Empty;

    /// <summary>
    /// Retrieves a list of viewable data (components and recipes) for a specific user, based on their view rights.
    /// If no user is provided, the current user is used. Filters out any components or recipes the user is restricted from viewing.
    /// </summary>
    /// <param name="user">Optional. The user for whom the viewable data is retrieved. If null, retrieves for the current user.</param>
    /// <param name="displayAllGroups">Optional. If true, retrieves data for all groups, even those the user might not have access to. Defaults to true.</param>
    /// <returns>A list of <see cref="IData"/> objects (components and recipes) the user is allowed to view, ordered by name.</returns>
    private List<IData> RetrieveViewableDataForUser(User? user = default, bool displayAllGroups = true)
    {
        user ??= MauiProgram.CurrentUser;
        return GetViewableComponents(user, displayAllGroups).Concat(GetViewableRecipes(user, displayAllGroups)).OrderBy(r => r.Name).ToList();
    }

    /// <summary>
    /// Retrieves the list of components the user is allowed to view, based on their view rights.
    /// Filters out any components the user does not have access to view.
    /// </summary>
    /// <param name="user">The user for whom the viewable components are retrieved.</param>
    /// <param name="displayAllGroups">Optional. If true, includes components from all groups, even those the user might not have access to. Defaults to true.</param>
    /// <returns>A list of viewable components as <see cref="IData"/>.</returns>
    private List<IData> GetViewableComponents(User user, bool displayAllGroups = true)
    {
        return (displayAllGroups) ? MauiProgram._context.Components
            .Where(c => !MauiProgram._context.MissingViewRightsComponents
                .Any(m => m.UserId == user.Id && m.ComponentId == c.Id && m.CannotSee))
            .Select(component => (IData)component)
            .ToList()
            : MauiProgram._context.Components
            .Where(c => !MauiProgram._context.MissingViewRightsComponents
                .Any(m => m.UserId == user.Id && m.ComponentId == c.Id && m.CannotSee)
                && !MauiProgram._context.MissingViewRightsGroups
                .Any(m => m.UserId == user.Id && m.GroupId == c.GroupId))
            .Select(component => (IData)component)
            .ToList();
    }

    /// <summary>
    /// Retrieves the list of recipes the user is allowed to view, based on their view rights.
    /// Filters out any recipes the user does not have access to view.
    /// </summary>
    /// <param name="user">The user for whom the viewable recipes are retrieved.</param>
    /// <param name="displayAllGroups">Optional. If true, includes recipes from all groups, even those the user might not have access to. Defaults to true.</param>
    /// <returns>A list of viewable recipes as <see cref="IData"/>.</returns>
    private List<IData> GetViewableRecipes(User user, bool displayAllGroups = true)
    {
        return (displayAllGroups) ? MauiProgram._context.Recipes
            .Where(r => !MauiProgram._context.MissingViewRightsRecipes
                .Any(m => m.UserId == user.Id && m.RecipeId == r.Id && m.CannotSee))
            .Select(recipe => (IData)recipe)
            .ToList()
            : MauiProgram._context.Recipes
            .Where(r => !MauiProgram._context.MissingViewRightsRecipes
                .Any(m => m.UserId == user.Id && m.RecipeId == r.Id && m.CannotSee)
                && !MauiProgram._context.MissingViewRightsGroups
                .Any(m => m.UserId == user.Id && m.GroupId == r.GroupId))
            .Select(recipe => (IData)recipe)
            .ToList();
    }

    /// <summary>
    /// Handles the completion of an entry in the search box or an change in the comboBox.
    /// Depending on the selected group type and search input, navigates to the appropriate search and view page.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnEntryCompleted(object sender, EventArgs e)
    {
        if (!_isInitialized)
            return;

        List<IData> results;
        IEnumerable<IData> baseQuery = TypeGroupPicker.SelectedIndex switch
        {
            (int)Selection.Components => GetViewableComponents(MauiProgram.CurrentUser),
            (int)Selection.Recipes => GetViewableRecipes(MauiProgram.CurrentUser),
            _ => RetrieveViewableDataForUser().Where(c => c.GroupId == TypeGroupPicker.SelectedIndex - (int)Selection.OffSet)
        };

        if (TypeGroupPicker.SelectedIndex == (int)Selection.All)
        {
            results = string.IsNullOrEmpty(SearchEntry.Text)
                ? RetrieveViewableDataForUser()
                : RetrieveViewableDataForUser().Where(c => c.Name.Contains(SearchEntry.Text) || (c.Description != null && c.Description.Contains(SearchEntry.Text))).ToList();
        }
        else
        {
            results = string.IsNullOrEmpty(SearchEntry.Text)
                ? baseQuery.ToList()
                : baseQuery.Where(c => c.Name.Contains(SearchEntry.Text) || (c.Description != null && c.Description.Contains(SearchEntry.Text))).ToList();
        }

        App.Current!.MainPage = new NavigationPage(new SearchAndViewPage(results.OrderBy(r => r.Name).ToList(), SearchEntry.Text, TypeGroupPicker.SelectedIndex));
    }

    /// <summary>
    /// Handles the tap event on a frame, navigates to the corresponding detail page based on the object's type.
    /// </summary>
    /// <param name="id">The unique identifier of the tapped object (either a Component or Recipe).</param>
    /// <param name="type">The type of the tapped object (Component or Recipe).</param>
    /// <exception cref="NotImplementedException">Thrown if the tapped object's type is not implemented.</exception>
    private static void OnFrameTapped(int id, Type type)
    {
        App.Current!.MainPage = type switch
        {
            Type t when t == typeof(Component) => new DetailPage(MauiProgram._context.Components.Single(c => c.Id == id)),
            Type t when t == typeof(Recipe) => new DetailPage(MauiProgram._context.Recipes.Single(c => c.Id == id)),
            _ => throw new NotImplementedException("type not implemented"),
        };
    }

    /// <summary>
    /// Handles the event to add a new group by showing a popup dialog for input.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnAddGroup(object sender, EventArgs e)
    {
        var popup = new AddGroupPopup();
        var result = (Group?)await this.ShowPopupAsync(popup);
        App.Current!.MainPage = new SearchAndViewPage();
    }

    /// <summary>
    /// Handles the event to add a new component by showing a popup dialog for input.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnAddComponent(object sender, EventArgs e)
    {
        var popup = new AddComponentPopup(MauiProgram._context.Users.Where(u => u.Id == Guid.Parse(MauiProgram.Configuration.GetSection("Connection:UserKey").Value!)).Single());
        var result = (Component?)await this.ShowPopupAsync(popup);
        App.Current!.MainPage = new SearchAndViewPage();
    }

    /// <summary>
    /// Handles the event to add a new recipe by showing a popup dialog for input.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnAddRecipe(object sender, EventArgs e)
    {
        var popup = new AddRecipePopup(MauiProgram._context.Users.Where(u => u.Id == Guid.Parse(MauiProgram.Configuration.GetSection("Connection:UserKey").Value!)).Single());
        var result = (Recipe?)await this.ShowPopupAsync(popup);
        App.Current!.MainPage = new SearchAndViewPage();
    }

    /// <summary>
    /// Navigates to the user overview page.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void OnViewUsers(object sender, EventArgs e)
    {
        App.Current!.MainPage = new UserOverviewPage();
    }

    /// <summary>
    /// Handles the event to add a new campaign by showing a popup dialog for input.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnAddCampaigns(object sender, EventArgs e)
    {
        var popup = new AddCampaignPopup();
        var result = (Campaign?)await this.ShowPopupAsync(popup);
        App.Current!.MainPage = new SearchAndViewPage();
    }

    /// <summary>
    /// Handles the event to open settings by showing the settings popup dialog.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnSettings(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new SettingsPopup(new SearchAndViewPage()));
    }
}