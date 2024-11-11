using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using RecipeCatalog.Data;
using RecipeCatalog.Helper;
using RecipeCatalog.Models;
using RecipeCatalog.Popups;
using RecipeCatalog.Resources.Language;

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
    /// <param name="search">Optional. The search text to pre-fill the search entry field.</param>
    /// <param name="selectedIndex">Optional. The index of the selected group type in the picker. Default is 0.</param>
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
            InitializeAsync(results, 10);
            _hasFilter = results != null;
            _isInitialized = true;
        }
    }

    /// <summary>
    /// Asynchronously initializes the view by loading and displaying viewable components and recipes based on the user's view rights.
    /// Retrieves a list of viewable items (components and recipes) from the database, processes them, and displays them in a grid layout with images, names, and descriptions.
    /// Handles filtering based on view rights for components, recipes, and categories.
    /// </summary>
    /// <param name="results">Optional. A list of <see cref="IData"/> objects (<see cref="Component"/> and <see cref="Recipe"/>) to be displayed. If null, retrieves the viewable items for the current user.</param>
    /// <returns>A task that represents the asynchronous operation of loading and displaying the items.</returns>
    private async Task InitializeAsync(List<IData>? results = null, int overrideTake = -1)
    {
        var viewableItems = results ?? await GetViewableItemsAsync(MauiProgram.CurrentUser, overrideTake: overrideTake);
        if (viewableItems.Count == 0)
            return;

        //TODO: ggf auslagern um nicht immer den aufruf zu machen ?
        var missingRightsComponent = MauiProgram._context.MissingViewRightsComponents.Where(m => m.UserId == MauiProgram.CurrentUser.Id).ToList();
        var missingRightsRecipe = MauiProgram._context.MissingViewRightsRecipes.Where(m => m.UserId == MauiProgram.CurrentUser.Id).ToList();
        var missingRightsCategory = MauiProgram._context.MissingViewRightsCategories.Where(m => m.UserId == MauiProgram.CurrentUser.Id).ToList();
        var bookmarks = MauiProgram._context.Bookmarks.Where(b => b.UserId == MauiProgram.CurrentUser.Id).ToList();


        for (int i = 0; i < (viewableItems.Count + numberOfColumns - 1) / numberOfColumns; i++)
        {
            ResultView.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        int realCounter = 0;
        for (int i = currentlyLoaded; i < currentlyLoaded + viewableItems.Count; i++)
        {
            var currentObject = viewableItems[realCounter];
            bool canAccessCategory = !missingRightsCategory.Any(g => g.CategoryId == currentObject.CategoryId);
            bool hasBookmark = currentObject switch
            {
                Component component => bookmarks.Any(b => b.ComponentId == component.Id),
                Recipe recipe => bookmarks.Any(b => b.RecipeId == recipe.Id),
                _ => false
            };
            string denied = "-";

            //needed for dispatcher
            int rowIndex = i / numberOfColumns;
            int columnIndex = i % numberOfColumns;

            // Dispatcher needed for ui
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var bookmark = new Button
                {
                    BackgroundColor = Color.FromHsla(0, 0, 0, 0),
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(1, 1),
                    ImageSource = new FontImageSource
                    {
                        Glyph = "\uf02e",
                        FontFamily = hasBookmark ? "FontAwesomeSolid" : "FontAwesomeRegular",
                        Size = 15,
                    },

                };

                var frame = new Frame
                {
                    BorderColor = canAccessCategory ? Color.Parse("Gray") : Color.Parse("DarkRed"),
                    CornerRadius = 5,
                    Padding = 10,
                    HasShadow = true,
                    BackgroundColor = Color.FromArgb("#50D3D3D3"),
                    Content = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        Children =
                        {
                            bookmark,
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
                                Text = canAccessCategory ? currentObject switch
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
                                Text = currentObject.CategoryId != null ?
                                    MauiProgram._context.Categories.Where(g => g.Id == currentObject.CategoryId).Select(g => g.CategoryName).Single()
                                    : string.Empty,
                                FontSize = 9,
                                TextColor = canAccessCategory ? Color.Parse("DarkGray") : Color.Parse("DarkRed"),
                            }
                        }
                    }
                };

                if (canAccessCategory)
                {
                    var tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += (s, e) => OnFrameTapped(currentObject.Id, currentObject.GetType());
                    frame.GestureRecognizers.Add(tapGestureRecognizer);
                }

                var tapGestureRecognizerBookmark = new TapGestureRecognizer();
                tapGestureRecognizerBookmark.Tapped += (s, e) => OnBookmarkTapped(currentObject.Id, currentObject.GetType(), hasBookmark, frame);
                bookmark.GestureRecognizers.Add(tapGestureRecognizerBookmark);

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
    /// <param name="displayAllCategories">Optional. If true, includes items from all groups, even those the user might not have access to. Defaults to true.</param>
    /// <param name="overrideTake">Optional. If -1 no override will take place. If 0, all entries are taken. Any positive number overrides the itemsToLoad attribute and starts from 0.</param>
    /// <returns>A task that represents the asynchronous operation, with a list of <see cref="IData"/> as the result, containing viewable components and recipes.</returns>
    /// <exception cref="Exception">Throws if any error occurs during the data retrieval or query execution.</exception>
    private async Task<List<IData>> GetViewableItemsAsync(User? user = default, bool displayAllCategories = true, int overrideTake = -1)
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
                    await GetViewableComponentsQuery(user, displayAllCategories).Select(c => new { c.Name, c.Id, Type = "C" }).Union(GetViewableRecipesQuery(user, displayAllCategories).Select(c => new { c.Name, c.Id, Type = "R" }))
                    .OrderBy(x => x.Name)
                    .Skip(currentlyLoaded)
                    .Take(itemsToLoad)
                    .ToListAsync()
                : (overrideTake == 0)
                    ?
                    await GetViewableComponentsQuery(user, displayAllCategories).Select(c => new { c.Name, c.Id, Type = "C" }).Union(GetViewableRecipesQuery(user, displayAllCategories).Select(c => new { c.Name, c.Id, Type = "R" }))
                    .OrderBy(x => x.Name)
                    .ToListAsync()
                    :
                    await GetViewableComponentsQuery(user, displayAllCategories).Select(c => new { c.Name, c.Id, Type = "C" }).Union(GetViewableRecipesQuery(user, displayAllCategories).Select(c => new { c.Name, c.Id, Type = "R" }))
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
    /// <param name="displayAllCategories">Optional. If true, includes components from all groups, even those the user might not have access to. Defaults to true.</param>
    /// <returns>An <see cref="IQueryable{IData}"/> representing the queryable list of viewable components.</returns>
    /// <exception cref="Exception">Throws if any error occurs while fetching or processing components data.</exception>
    private IQueryable<IData> GetViewableComponentsQuery(User user, bool displayAllCategories = true)
    {
        return (displayAllCategories) ? MauiProgram._context.Components
            .Where(c => !MauiProgram._context.MissingViewRightsComponents
                .Any(m => m.UserId == user.Id && m.ComponentId == c.Id && m.CannotSee))
            : MauiProgram._context.Components
            .Where(c => !MauiProgram._context.MissingViewRightsComponents
                .Any(m => m.UserId == user.Id && m.ComponentId == c.Id && m.CannotSee)
                && !MauiProgram._context.MissingViewRightsCategories
                .Any(m => m.UserId == user.Id && m.CategoryId == c.CategoryId));
    }

    /// <summary>
    /// Retrieves a queryable list of recipes the user is allowed to view, based on their view rights.
    /// This query can be further processed before executing it, allowing for additional filtering or pagination.
    /// Filters out any recipes the user does not have access to view.
    /// </summary>
    /// <param name="user">The user for whom the viewable recipes are retrieved.</param>
    /// <param name="displayAllCategories">Optional. If true, includes recipes from all groups, even those the user might not have access to. Defaults to true.</param>
    /// <returns>An <see cref="IQueryable{IData}"/> representing the queryable list of viewable recipes.</returns>
    /// <exception cref="Exception">Throws if any error occurs while fetching or processing recipes data.</exception>
    private IQueryable<IData> GetViewableRecipesQuery(User user, bool displayAllCategories = true)
    {
        return (displayAllCategories) ? MauiProgram._context.Recipes
            .Where(r => !MauiProgram._context.MissingViewRightsRecipes
                .Any(m => m.UserId == user.Id && m.RecipeId == r.Id && m.CannotSee))
            : MauiProgram._context.Recipes
            .Where(r => !MauiProgram._context.MissingViewRightsRecipes
                .Any(m => m.UserId == user.Id && m.RecipeId == r.Id && m.CannotSee)
                && !MauiProgram._context.MissingViewRightsCategories
                .Any(m => m.UserId == user.Id && m.CategoryId == r.CategoryId));
    }

    /// <summary>
    /// Retrieves a list of items (Components or Recipes) that have been bookmarked by the specified user.
    /// The results are filtered based on the provided parameters such as category display options and pagination.
    /// </summary>
    /// <param name="user">The user whose bookmarks are being retrieved.</param>
    /// <param name="displayAllCategories">A flag to determine whether to display all categories (default is true).</param>
    /// <param name="overrideTake">An optional parameter to limit the number of results. If -1, all results are returned; 0 returns all matching items; otherwise, it limits the number of results returned.</param>
    /// <returns>A list of IData objects representing the bookmarked items (either Components or Recipes).</returns>
    /// <exception cref="Exception">Throws if an error occurs while fetching the data.</exception>
    private async Task<List<IData>> GetBookmarkedItemsQuery(User user, bool displayAllCategories = true, int overrideTake = -1)
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
                    await GetViewableComponentsQuery(user, displayAllCategories).Where(c => MauiProgram._context.Bookmarks.Any(b => b.ComponentId == c.Id)).Select(c => new { c.Name, c.Id, Type = "C" }).Union(GetViewableRecipesQuery(user, displayAllCategories).Where(c => MauiProgram._context.Bookmarks.Any(b => b.RecipeId == c.Id)).Select(c => new { c.Name, c.Id, Type = "R" }))
                    .OrderBy(x => x.Name)
                    .Skip(currentlyLoaded)
                    .Take(itemsToLoad)
                    .ToListAsync()
                : (overrideTake == 0)
                    ?
                    await GetViewableComponentsQuery(user, displayAllCategories).Where(c => MauiProgram._context.Bookmarks.Any(b => b.ComponentId == c.Id)).Select(c => new { c.Name, c.Id, Type = "C" }).Union(GetViewableRecipesQuery(user, displayAllCategories).Where(c => MauiProgram._context.Bookmarks.Any(b => b.RecipeId == c.Id)).Select(c => new { c.Name, c.Id, Type = "R" }))
                    .OrderBy(x => x.Name)
                    .ToListAsync()
                    :
                    await GetViewableComponentsQuery(user, displayAllCategories).Where(c => MauiProgram._context.Bookmarks.Any(b => b.ComponentId == c.Id)).Select(c => new { c.Name, c.Id, Type = "C" }).Union(GetViewableRecipesQuery(user, displayAllCategories).Where(c => MauiProgram._context.Bookmarks.Any(b => b.RecipeId == c.Id)).Select(c => new { c.Name, c.Id, Type = "R" }))
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
            if (result.Count == 0)
            {
                LoadingSpinner.IsRunning = false;
                LoadingSpinner.IsVisible = false;
                _isLoading = false;
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
        return result;
    }

    /// <summary>
    /// Populates the picker component with a list of categories, including predefined options for filtering Bookmarks, Components, and Recipes.
    /// The selected index is set, and the search text is pre-filled into the search entry field.
    /// </summary>
    /// <param name="search">The text to display in the search entry, allowing users to pre-fill their search.</param>
    /// <param name="selectedIndex">The index of the selected category to be set in the picker. This allows the picker to highlight the correct filter option on load.</param>
    private void LoadPickerData(string search, int selectedIndex)
{
        SearchEntry.Text = search;

        List<Category> entrys = new List<Category>() { new() { CategoryName = "-" }, new() { CategoryName = AppLanguage.Filter_Bookmarks }, new() { CategoryName = AppLanguage.Filter_Components }, new() { CategoryName = AppLanguage.Filter_Recipes } };
        entrys.AddRange(MauiProgram._context.Categories.ToList());
        TypeCategoryPicker.ItemsSource = entrys;
        TypeCategoryPicker.ItemDisplayBinding = new Binding(nameof(Category.CategoryName));
        TypeCategoryPicker.SelectedIndex = selectedIndex;
    }

    /// <summary>
    /// Truncates a given description string to a specified length, appending ellipses ("...") if the description exceeds the given length.
    /// If the description is null or doesn't exceed the length, it is returned as is.
    /// </summary>
    /// <param name="desc">The description text that needs to be truncated.</param>
    /// <param name="length">The maximum allowed length for the description text. Defaults to 200 characters.</param>
    /// <returns>A truncated description string with ellipses if it exceeds the specified length, or the original description if within the limit.</returns>
    private string GetTruncatedDescription(string? desc, int length = 200) => desc != null && desc.Length > length ? desc.Substring(0, length) + "..." : desc ?? string.Empty;

    /// <summary>
    /// Triggered when the user completes an entry in the search field or changes the selected category in the combo box.
    /// Based on the search input and selected category, this method fetches matching items (Components or Recipes) and navigates to the appropriate search view page.
    /// The method also ensures that data is filtered based on the search query and selected category, while managing pagination for large result sets.
    /// </summary>
    /// <param name="sender">The source of the event, usually the search entry component.</param>
    /// <param name="e">The event data, which can provide additional context for the trigger (e.g., search completion).</param>
    private async void OnEntryCompleted(object sender, EventArgs e)
    { 
        if (!_isInitialized || _isLoading)
            return;

        string search = SearchEntry.Text.Trim().ToLower();
        int overrideTake = ((search == null || search == string.Empty) && TypeCategoryPicker.SelectedIndex == (int)Selection.All) ? -1 : 0;
        List<IData> results;
        IEnumerable<IData> baseQuery = null;
        if (TypeCategoryPicker.SelectedIndex == (int)Selection.All && overrideTake == -1)
        {
            App.Current!.MainPage = new SearchAndViewPage();
            return;//savety
        }

        if (TypeCategoryPicker.SelectedIndex == (int)Selection.All)
        {
            var items = await GetViewableItemsAsync(overrideTake: overrideTake);
            results = string.IsNullOrEmpty(search)
                ? items
                : items.SearchFilter(search);
        }
        else if (TypeCategoryPicker.SelectedIndex == (int)Selection.Bookmarks)
        {
            var items = await GetBookmarkedItemsQuery(MauiProgram.CurrentUser, overrideTake: overrideTake);
            results = string.IsNullOrEmpty(search)
                ? items
                : items.SearchFilter(search);
        }
        else 
        {
            if (TypeCategoryPicker.SelectedIndex == (int)Selection.Components)
            {
                baseQuery = GetViewableComponentsQuery(MauiProgram.CurrentUser);
            }
            else if (TypeCategoryPicker.SelectedIndex == (int)Selection.Recipes)
            {
                baseQuery = GetViewableRecipesQuery(MauiProgram.CurrentUser);
            }
            else
            {
                var items = await GetViewableItemsAsync(overrideTake: overrideTake);
                baseQuery = items.Where(c => c.CategoryId == TypeCategoryPicker.SelectedIndex - (int)Selection.OffSet);
            }
            results = string.IsNullOrEmpty(search)
                ? baseQuery.ToList()
                : baseQuery.SearchFilter(search);
        }

        //started in GetViewableItemsAsync
        _isLoading = true;
        LoadingSpinner.IsRunning = true;
        LoadingSpinner.IsVisible = true;
        App.Current!.MainPage = new SearchAndViewPage(results.OrderBy(r => r.Name).ToList(), SearchEntry.Text.Trim(), TypeCategoryPicker.SelectedIndex);
    }

    /// <summary>
    /// Handles the tap event on a UI frame and navigates to the detail page corresponding to the tapped object (either Component or Recipe).
    /// The method checks the type of the tapped object and navigates to the appropriate detail page based on the provided object ID.
    /// If the type is not supported, an exception is thrown.
    /// </summary>
    /// <param name="id">The unique identifier of the tapped object (either a Component or Recipe).</param>
    /// <param name="type">The type of the tapped object (either Component or Recipe) that dictates which detail page to navigate to.</param>
    /// <exception cref="NotImplementedException">Thrown if the object's type is not supported by the navigation logic.</exception>
    private static void OnFrameTapped(int id, Type type)
    {
        try
        {
            App.Current!.MainPage = type switch
            {
                Type t when t == typeof(Component) => new DetailPage(MauiProgram._context.Components.Single(c => c.Id == id)),
                Type t when t == typeof(Recipe) => new DetailPage(MauiProgram._context.Recipes.Single(c => c.Id == id)),
                _ => throw new NotImplementedException("type not implemented"),
            };
        }
        catch (Exception ex)
        {
            //TODO: DoLater
        }
    }

    /// <summary>
    /// Handles the event when a bookmark icon is tapped. It adds or removes a bookmark for the given object (Component or Recipe)
    /// based on the current bookmark status. If the object is already bookmarked, it removes the bookmark; otherwise, it adds a new bookmark.
    /// After the change, the frame's content is updated to reflect the new bookmark status.
    /// </summary>
    /// <param name="id">The unique identifier of the object (either a Component or Recipe) being bookmarked or unbookmarked.</param>
    /// <param name="type">The type of the object (either Component or Recipe) being bookmarked.</param>
    /// <param name="currentlyMarked">A flag indicating whether the item is already bookmarked. If true, the item will be unbookmarked; if false, it will be bookmarked.</param>
    /// <param name="frame">The UI frame that contains the bookmark button, which will be updated to reflect the new bookmark status.</param>
    /// <exception cref="NotImplementedException">Thrown if the object type is not supported for bookmarking.</exception>
    private static void OnBookmarkTapped(int id, Type type, bool currentlyMarked, Frame frame)
    {
        try
        {
            if (currentlyMarked)
            {
                var mark = type switch
                {
                    Type t when t == typeof(Component) => MauiProgram._context.Bookmarks.Single(c => c.UserId == MauiProgram.CurrentUser.Id && c.ComponentId == id),
                    Type t when t == typeof(Recipe) => MauiProgram._context.Bookmarks.Single(c => c.UserId == MauiProgram.CurrentUser.Id && c.RecipeId == id),
                    _ => throw new NotImplementedException("type not implemented"),
                };
                MauiProgram._context.Bookmarks.Remove(mark);
            }
            else
            {
                Bookmark mark = new()
                {
                    UserId = MauiProgram.CurrentUser.Id,
                };
                switch (type)
                {
                    case Type t when t == typeof(Component):
                        mark.ComponentId = id;
                        break;
                    case Type t when t == typeof(Recipe):
                        mark.RecipeId = id;
                        break;
                    case Type _:
                        throw new NotImplementedException("type not implemented");
                }
                MauiProgram._context.Bookmarks.Add(mark);
            }
            MauiProgram._context.SaveChanges();
            UpdateFrameContent(frame, !currentlyMarked, id, type);
        }
        catch (Exception ex)
        {
            //TODO: DoLater
        }
    }

    /// <summary>
    /// Updates the content of a frame based on the current bookmark status of the object.
    /// The bookmark button's image source is updated to show either the "solid" or "regular" bookmark icon depending on whether the item is now bookmarked.
    /// It also resets the tap gesture recognizer on the bookmark button to allow toggling the bookmark status.
    /// </summary>
    /// <param name="frame">The frame that contains the bookmark button to be updated.</param>
    /// <param name="isNowMarked">A flag indicating whether the object is now bookmarked (true) or unbookmarked (false).</param>
    /// <param name="id">The unique identifier of the object being bookmarked or unbookmarked.</param>
    /// <param name="type">The type of the object (Component or Recipe) being updated.</param>
    private static void UpdateFrameContent(Frame frame, bool isNowMarked, int id, Type type)
    {
        var stackLayout = frame.Content as StackLayout;
        if (stackLayout != null)
        {
            var buttonBookmark = stackLayout.Children.OfType<Button>().FirstOrDefault();
            if (buttonBookmark != null)
            {
                buttonBookmark.ImageSource = new FontImageSource
                {
                    Glyph = "\uf02e",
                    FontFamily = isNowMarked ? "FontAwesomeSolid" : "FontAwesomeRegular",
                    Size = 15,
                };
                buttonBookmark.GestureRecognizers.Clear();
                var tapGestureRecognizerBookmark = new TapGestureRecognizer();
                tapGestureRecognizerBookmark.Tapped += (s, e) => OnBookmarkTapped(id, type, isNowMarked, frame);
                buttonBookmark.GestureRecognizers.Add(tapGestureRecognizerBookmark);
            }
        }
    }

    /// <summary>
    /// Handles the event to add a new category by showing a popup dialog for category input.
    /// Once the new category is added, the main page is refreshed to show the updated view.
    /// </summary>
    /// <param name="sender">The sender of the event, usually a UI element that triggers the category addition.</param>
    /// <param name="e">Event arguments containing data about the event.</param>
    private async void OnAddCategory(object sender, EventArgs e)
    {
        var popup = new AddCategoryPopup();
        var result = (Category?)await this.ShowPopupAsync(popup);
        App.Current!.MainPage = new SearchAndViewPage();
    }

    /// <summary>
    /// Handles the event to add a new component by showing a popup dialog for component input.
    /// Once the component is added, the main page is refreshed to show the updated view.
    /// </summary>
    /// <param name="sender">The sender of the event, usually a UI element that triggers the component addition.</param>
    /// <param name="e">Event arguments containing data about the event.</param>
    private async void OnAddComponent(object sender, EventArgs e)
    {
        var popup = new AddComponentPopup(MauiProgram._context.Users.Where(u => u.Id == MauiProgram.CurrentUser.Id).Single());
        var result = (Component?)await this.ShowPopupAsync(popup);
        App.Current!.MainPage = new SearchAndViewPage();
    }

    /// <summary>
    /// Handles the event to add a new recipe by showing a popup dialog for recipe input.
    /// Once the recipe is added, the main page is refreshed to show the updated view.
    /// </summary>
    /// <param name="sender">The sender of the event, usually a UI element that triggers the recipe addition.</param>
    /// <param name="e">Event arguments containing data about the event.</param>
    private async void OnAddRecipe(object sender, EventArgs e)
    {
        var popup = new AddRecipePopup(MauiProgram._context.Users.Where(u => u.Id == MauiProgram.CurrentUser.Id).Single());
        var result = (Recipe?)await this.ShowPopupAsync(popup);
        App.Current!.MainPage = new SearchAndViewPage();
    }

    /// <summary>
    /// Navigates to the user overview page when triggered by the user interaction (e.g., button tap).
    /// This method will switch the main page to the user overview screen, allowing the user to view and manage other users.
    /// </summary>
    /// <param name="sender">The sender of the event, typically the UI element that triggers the navigation (e.g., a button).</param>
    /// <param name="e">Event arguments containing data about the event.</param>
    private void OnViewUsers(object sender, EventArgs e)
    {
        App.Current!.MainPage = new UserOverviewPage();
    }

    /// <summary>
    /// Handles the event to add a new campaign by showing a popup dialog for campaign input.
    /// Once the campaign is added, the main page is refreshed to show the updated view.
    /// </summary>
    /// <param name="sender">The sender of the event, usually a UI element that triggers the campaign addition.</param>
    /// <param name="e">Event arguments containing data about the event.</param>
    private async void OnAddCampaigns(object sender, EventArgs e)
    {
        var popup = new AddCampaignPopup();
        var result = (Campaign?)await this.ShowPopupAsync(popup);
        App.Current!.MainPage = new SearchAndViewPage();
    }

    /// <summary>
    /// Opens the settings page by showing the settings popup dialog.
    /// Once the user interacts with the settings popup, it will return the user to the previous screen (SearchAndViewPage).
    /// </summary>
    /// <param name="sender">The sender of the event, usually the UI element that triggers the settings view (e.g., a button).</param>
    /// <param name="e">Event arguments containing data about the event.</param>
    private async void OnSettings(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new SettingsPopup(new SearchAndViewPage()));
    }

    /// <summary>
    /// Handles the scrolling event of the result scroll view. If the user scrolls close to the bottom of the scroll view and no other loading operation is in progress,
    /// it triggers the initialization of additional data loading (e.g., for pagination or infinite scrolling).
    /// </summary>
    /// <param name="sender">The source of the event, typically the scroll view component that is being scrolled.</param>
    /// <param name="e">Event arguments containing the scroll position, which helps determine if the user has scrolled near the bottom of the content.</param>
    private async void OnResultScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        if (!_isLoading && !_hasFilter && (e.ScrollY >= ResultScrollView.ContentSize.Height - ResultScrollView.Height - 100))
        {
            InitializeAsync(overrideTake: 4);
        }
    }
}