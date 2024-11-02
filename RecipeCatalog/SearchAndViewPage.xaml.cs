using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using RecipeCatalog.Data;
using RecipeCatalog.Models;
using RecipeCatalog.Popups;
using RecipeCatalog.Resources.Language;
using System.Linq;

namespace RecipeCatalog;

public partial class SearchAndViewPage : ContentPage
{
    private readonly bool _isInitialized = false;
    private const int itemsToLoad = 10;
    private int currentlyLoaded = 0;
    private int numberOfColumns = 2;
    private bool _isLoading = false;
    private bool _hasFilter = false;

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
        if(MauiProgram.CurrentUser.IsAdmin)
            AdminArea.IsVisible = true;

        if (MauiProgram.CurrentUser.IsAdmin || MauiProgram.CurrentUser.CampaignId != null)
        {
            LoadPickerData(search, selectedIndex);
            for (int i = 0; i < numberOfColumns; i++)
            {
                ResultView.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }
            InitializeAsync(results);
            _hasFilter = results != null;
            _isInitialized = true;
        }
    }

    /// <summary>
    /// Asynchronously initializes the view by loading and displaying viewable components and recipes based on the user's view rights.
    /// Retrieves a list of viewable items (components and recipes) from the database, processes them, and displays them in a grid layout with images, names, and descriptions.
    /// Handles filtering based on view rights for components, recipes, and groups.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task InitializeAsync(List<IData>? results = null)
    {
        var viewableItems = results ?? await GetViewableItemsAsync(MauiProgram.CurrentUser);
        if (viewableItems.Count == 0)
            return;

        //TODO: ggf auslagern um nicht immer den aufruf zu machen ?
        var missingRightsComponent = MauiProgram._context.MissingViewRightsComponents.Where(m => m.UserId == MauiProgram.CurrentUser.Id).ToList();
        var missingRightsRecipe = MauiProgram._context.MissingViewRightsRecipes.Where(m => m.UserId == MauiProgram.CurrentUser.Id).ToList();
        var missingRightsGroup = MauiProgram._context.MissingViewRightsGroups.Where(m => m.UserId == MauiProgram.CurrentUser.Id).ToList();


        for (int i = 0; i < (viewableItems.Count + numberOfColumns - 1) / numberOfColumns; i++)
        {
            ResultView.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        int realCounter = 0;
        for (int i = currentlyLoaded; i < currentlyLoaded + viewableItems.Count; i++)
        {
            var currentObject = viewableItems[realCounter];
            bool canAccessGroup = !missingRightsGroup.Any(g => g.GroupId == currentObject.GroupId);
            string denied = "-";

            //needed for dispatcher
            int rowIndex = i / numberOfColumns;
            int columnIndex = i % numberOfColumns;

            // Dispatcher needed for ui
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
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
                        BackgroundColor = Color.FromHsla(0, 0, 0, 0),
                        HeightRequest = 100,
                        WidthRequest = 100,
                        BorderColor = Color.FromHsla(0, 0, 0, 0),
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
                        Text = canAccessGroup ? currentObject switch
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
                        Text = currentObject.GroupId != null ?
                            MauiProgram._context.Groups.Where(g => g.Id == currentObject.GroupId).Select(g => g.GroupName).Single()
                            : string.Empty,
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
                Grid.SetRow(frame, rowIndex);
                Grid.SetColumn(frame, columnIndex);
            });
            realCounter++;
        }
        //started in GetViewableItemsAsync
        LoadingSpinner.IsRunning = false;
        LoadingSpinner.IsVisible = false;
        _isLoading = false;
        currentlyLoaded += viewableItems.Count();
    }

    /// <summary>
    /// Asynchronously retrieves a list of viewable components and recipes for a specified user, 
    /// filtered by their access rights and group visibility settings. The results are paginated based on the 
    /// number of items to load and the current number of loaded items.
    /// </summary>
    /// <param name="user">Optional. The user for whom the viewable items are retrieved. If null, the current user is used.</param>
    /// <param name="displayAllGroups">Optional. If true, includes items from all groups, even those the user might not have access to. Defaults to true.</param>
    /// <param name="overrideTake">Optional. If -1 no override will take place. If 0 all entrys are taken and all other positive numbers will override the itemsToLoad attribute and start from 0</param>
    /// <returns>A task that represents the asynchronous operation, with a list of <see cref="IData"/> as the result, containing viewable components and recipes.</returns>
    private async Task<List<IData>> GetViewableItemsAsync(User? user = default, bool displayAllGroups = true, int overrideTake = -1)
    {
        List<IData> result = new();
        try
        {
            //stopped in InitializeAsync or OnEntryCompleted or here when no results
            _isLoading = true;
            LoadingSpinner.IsRunning = true;
            LoadingSpinner.IsVisible = true;
            user ??= MauiProgram.CurrentUser;
            var res = (overrideTake == -1) 
                ?
                    await GetViewableComponentsQuery(user, displayAllGroups).Select(c => new { c.Name, c.Id, Type = "C" }).Union(GetViewableRecipesQuery(user, displayAllGroups).Select(c => new { c.Name, c.Id, Type = "R" }))
                    .OrderBy(x => x.Name)
                    .Skip(currentlyLoaded)
                    .Take(itemsToLoad)
                    .ToListAsync()
                : (overrideTake == 0)
                    ?
                    await GetViewableComponentsQuery(user, displayAllGroups).Select(c => new { c.Name, c.Id, Type = "C" }).Union(GetViewableRecipesQuery(user, displayAllGroups).Select(c => new { c.Name, c.Id, Type = "R" }))
                    .OrderBy(x => x.Name)
                    .ToListAsync()
                    :
                    await GetViewableComponentsQuery(user, displayAllGroups).Select(c => new { c.Name, c.Id, Type = "C" }).Union(GetViewableRecipesQuery(user, displayAllGroups).Select(c => new { c.Name, c.Id, Type = "R" }))
                    .OrderBy(x => x.Name)
                    .Take(overrideTake)
                    .ToListAsync()
            ;

            foreach (var item in res)
            {
                if (item.Type == "C")
                    result.Add(await MauiProgram._context.Components.Where(c => c.Id == item.Id).SingleAsync());
                else
                    result.Add(await MauiProgram._context.Recipes.Where(c => c.Id == item.Id).SingleAsync());
            }
            if(result.Count == 0)
            {
                LoadingSpinner.IsRunning = false;
                LoadingSpinner.IsVisible = false;
                _isLoading = false;
            }
        }
        catch(Exception ex)
        {
            throw new Exception(ex.Message);
        }
        return result;
    }

    /// <summary>
    /// Retrieves a queryable list of components the user is allowed to view, based on their view rights.
    /// This query can be further processed before executing it, allowing for additional filtering or pagination.
    /// Filters out any components the user does not have access to view.
    /// </summary>
    /// <param name="user">The user for whom the viewable components are retrieved.</param>
    /// <param name="displayAllGroups">Optional. If true, includes components from all groups, even those the user might not have access to. Defaults to true.</param>
    /// <returns>An <see cref="IQueryable{IData}"/> representing the queryable list of viewable components.</returns>
    private IQueryable<IData> GetViewableComponentsQuery(User user, bool displayAllGroups = true)
    {
        return (displayAllGroups) ? MauiProgram._context.Components
            .Where(c => !MauiProgram._context.MissingViewRightsComponents
                .Any(m => m.UserId == user.Id && m.ComponentId == c.Id && m.CannotSee))
            : MauiProgram._context.Components
            .Where(c => !MauiProgram._context.MissingViewRightsComponents
                .Any(m => m.UserId == user.Id && m.ComponentId == c.Id && m.CannotSee)
                && !MauiProgram._context.MissingViewRightsGroups
                .Any(m => m.UserId == user.Id && m.GroupId == c.GroupId));
    }

    /// <summary>
    /// Retrieves a queryable list of recipes the user is allowed to view, based on their view rights.
    /// This query can be further processed before executing it, allowing for additional filtering or pagination.
    /// Filters out any recipes the user does not have access to view.
    /// </summary>
    /// <param name="user">The user for whom the viewable recipes are retrieved.</param>
    /// <param name="displayAllGroups">Optional. If true, includes recipes from all groups, even those the user might not have access to. Defaults to true.</param>
    /// <returns>An <see cref="IQueryable{IData}"/> representing the queryable list of viewable recipes.</returns>
    private IQueryable<IData> GetViewableRecipesQuery(User user, bool displayAllGroups = true)
    {
        return (displayAllGroups) ? MauiProgram._context.Recipes
            .Where(r => !MauiProgram._context.MissingViewRightsRecipes
                .Any(m => m.UserId == user.Id && m.RecipeId == r.Id && m.CannotSee))
            : MauiProgram._context.Recipes
            .Where(r => !MauiProgram._context.MissingViewRightsRecipes
                .Any(m => m.UserId == user.Id && m.RecipeId == r.Id && m.CannotSee)
                && !MauiProgram._context.MissingViewRightsGroups
                .Any(m => m.UserId == user.Id && m.GroupId == r.GroupId));
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
    /// Truncates the description text to the specified length and adds ellipsis if the text exceeds the limit.
    /// </summary>
    /// <param name="desc">The description text to be truncated.</param>
    /// <param name="length">Optional. The maximum length of the truncated description. Defaults to 200 characters.</param>
    /// <returns>A truncated description with ellipsis if it exceeds the specified length, or the full description if it's within the limit.</returns>
    private string GetTruncatedDescription(string? desc, int length = 200) => desc != null && desc.Length > length ? desc.Substring(0, length) + "..." : desc ?? string.Empty; 

    /// <summary>
    /// Handles the completion of an entry in the search box or an change in the comboBox.
    /// Depending on the selected group type and search input, navigates to the appropriate search and view page.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private async void OnEntryCompleted(object sender, EventArgs e)
    { 
        if (!_isInitialized || _isLoading)
            return;

        string search = SearchEntry.Text.Trim().ToLower();
        int overrideTake = ((search == null || search == string.Empty) && TypeGroupPicker.SelectedIndex == (int)Selection.All) ? -1 : 0;
        List<IData> results;
        IEnumerable<IData> baseQuery = null;
        if (TypeGroupPicker.SelectedIndex == (int)Selection.All && overrideTake == -1)
        {
            App.Current!.MainPage = new SearchAndViewPage();
            return;//savety
        }

        if (TypeGroupPicker.SelectedIndex == (int)Selection.All)
        {
            var items = await GetViewableItemsAsync(overrideTake: overrideTake);
            results = string.IsNullOrEmpty(search)
                ? items
                : items.Where(c => c.Name.ToLower().Contains(search) || (c.Description != null && c.Description.ToLower().Contains(search))).ToList();
        }
        else
        {
            if (TypeGroupPicker.SelectedIndex == (int)Selection.Components)
            {
                baseQuery = GetViewableComponentsQuery(MauiProgram.CurrentUser);
            }
            else if (TypeGroupPicker.SelectedIndex == (int)Selection.Recipes)
            {
                baseQuery = GetViewableRecipesQuery(MauiProgram.CurrentUser);
            }
            else
            {
                var items = await GetViewableItemsAsync(overrideTake: overrideTake);
                baseQuery = items.Where(c => c.GroupId == TypeGroupPicker.SelectedIndex - (int)Selection.OffSet);
            }
            results = string.IsNullOrEmpty(search)
                ? baseQuery.ToList()
                : baseQuery.Where(c => c.Name.ToLower().Contains(search) || (c.Description != null && c.Description.ToLower().Contains(search))).ToList();
        }

        //started in GetViewableItemsAsync
        _isLoading = true;
        LoadingSpinner.IsRunning = true;
        LoadingSpinner.IsVisible = true;
        App.Current!.MainPage = new SearchAndViewPage(results.OrderBy(r => r.Name).ToList(), SearchEntry.Text.Trim(), TypeGroupPicker.SelectedIndex);
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
        var popup = new AddComponentPopup(MauiProgram._context.Users.Where(u => u.Id == MauiProgram.CurrentUser.Id).Single());
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
        var popup = new AddRecipePopup(MauiProgram._context.Users.Where(u => u.Id == MauiProgram.CurrentUser.Id).Single());
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

    /// <summary>
    /// Handles the scrolling event of the result scroll view. Triggers the loading of additional
    /// data when the user scrolls close to the bottom of the scroll view, provided that no loading 
    /// operation is currently in progress.
    /// </summary>
    /// <param name="sender">The source of the event, typically the scroll view that is being scrolled.</param>
    /// <param name="e">An event argument containing data about the scroll event, including the scroll position.</param>
    private async void OnResultScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        if (!_isLoading && !_hasFilter && (e.ScrollY >= ResultScrollView.ContentSize.Height - ResultScrollView.Height - 100))
        {
            InitializeAsync();
        }
    }
}