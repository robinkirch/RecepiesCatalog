using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using RecipeCatalog.Data;
using RecipeCatalog.Models;
using RecipeCatalog.Popups;
using RecipeCatalog.Resources.Language;
using System.Globalization;
using System.Resources;

namespace RecipeCatalog;

public partial class SearchAndViewPage : ContentPage
{
    private bool _isInitialized = false;
    private System.Threading.Timer? _debounceTimer;
    private const int DebounceInterval = 2000;
    public SearchAndViewPage(List<IData>? results = null, string search = "", int selectedIndex = 0)
	{
		InitializeComponent();
        _isInitialized = false;
        if (MauiProgram.configuration.GetSection("Connection:SecretKey").Value != "+KJDS??oO(D=)o8d-ü3=lkdsa3!3")
            AdminArea.IsVisible = false;

        LoadPickerData(search, selectedIndex);
        LoadView(results);
        _isInitialized = true;
    }

    private void LoadPickerData(string search, int selectedIndex)
    {
        SearchEntry.Text = search;

        List<Group> entrys = new List<Group>() { new() { GroupName = "-" }, new() { GroupName = "Components"}, new() { GroupName = "Recipes" } };
        entrys.AddRange(MauiProgram._context.Groups.ToList());
        TypeGroupPicker.ItemsSource = entrys;
        TypeGroupPicker.ItemDisplayBinding = new Binding("GroupName");
        TypeGroupPicker.SelectedIndex = selectedIndex;
    }

    private void LoadView(List<IData>? results = null)
    {
        if (results == null)
        {
            results = MauiProgram._context.Components.Select(component => (IData)component).ToList();
            results.AddRange(MauiProgram._context.Recipes.Select(component => (IData)component).ToList());
            results = results.OrderBy(r => r.Name).ToList();
        }

        int numberOfColumns = 2;
        for (int i = 0; i < numberOfColumns; i++)
        {
            ResultView.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        }

        int numberOfRows = (results.Count + numberOfColumns - 1) / numberOfColumns; // Runden auf die nächste ganze Zahl
        for (int i = 0; i < numberOfRows; i++)
        {
            ResultView.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        // Füge Frames hinzu
        for (int i = 0; i < results.Count; i++)
        {
            var c = results[i];

            // Erstellen des Objekts als Frame
            var frame = new Frame
            {
                BorderColor = Color.Parse("Gray"),
                CornerRadius = 5,
                Padding = 10,
                HasShadow = true,
                BackgroundColor = Color.FromArgb("#50D3D3D3"),
                Content = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    Children =
                {
                    new Image
                    {
                        Source = MauiProgram.ByteArrayToImageSource(c.Image),
                        Aspect = Aspect.AspectFit,
                        HeightRequest = 100
                    },
                    new Label
                    {
                        Text = c.Name,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 16
                    },
                    new Label
                    {
                        Text = c.Description,
                        FontSize = 14
                    },
                    new Label
                    {
                        Text = c.GroupId.ToString(),
                        FontSize = 10
                    }
                }
                }
            };

            // Füge TapGestureRecognizer hinzu
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => OnFrameTapped(c.Id, c.GetType());
            frame.GestureRecognizers.Add(tapGestureRecognizer);

            // Bestimme Zeile und Spalte
            int row = i / numberOfColumns;
            int col = i % numberOfColumns;

            // Füge Frame zum Grid hinzu
            ResultView.Children.Add(frame);
            Grid.SetRow(frame, row);
            Grid.SetColumn(frame, col);
        }
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        if (!_isInitialized)
            return;

        //offset for filter groups
        int offset = 2;

        if (TypeGroupPicker.SelectedIndex == 0)
        {
            //all
            if (SearchEntry.Text.Length == 0)
            {
                App.Current.MainPage = new SearchAndViewPage();
            }
            else
            {
                List<IData> results = MauiProgram._context.Components.Where(c => c.Name.Contains(SearchEntry.Text) || c.Description.Contains(SearchEntry.Text)).Select(component => (IData)component).ToList();
                results.AddRange(MauiProgram._context.Recipes.Where(c => c.Name.Contains(SearchEntry.Text) || c.Description.Contains(SearchEntry.Text)).Select(component => (IData)component).ToList());
                results = results.OrderBy(r => r.Name).ToList();
                App.Current.MainPage = new SearchAndViewPage(results, SearchEntry.Text);
            }
        }
        else
        {
            if (SearchEntry.Text.Length == 0)
            {
                //Components
                if(TypeGroupPicker.SelectedIndex == 1)
                {
                    List<IData> results = MauiProgram._context.Components.Select(c => (IData)c).OrderBy(r => r.Name).ToList();
                    App.Current.MainPage = new SearchAndViewPage(results, selectedIndex: TypeGroupPicker.SelectedIndex);
                }
                //Recipes
                else if(TypeGroupPicker.SelectedIndex == 2)
                {
                    List<IData> results = MauiProgram._context.Recipes.Select(r => (IData)r).OrderBy(r => r.Name).ToList();
                    App.Current.MainPage = new SearchAndViewPage(results, selectedIndex: TypeGroupPicker.SelectedIndex);
                }
                //groups
                else
                {
                    List<IData> results = MauiProgram._context.Components.Where(c => c.GroupId == TypeGroupPicker.SelectedIndex - offset).Select(component => (IData)component).ToList();
                    results.AddRange(MauiProgram._context.Recipes.Where(c => c.GroupId == TypeGroupPicker.SelectedIndex - offset).Select(component => (IData)component).ToList());
                    results = results.OrderBy(r => r.Name).ToList();
                    App.Current.MainPage = new SearchAndViewPage(results, selectedIndex: TypeGroupPicker.SelectedIndex);
                }
            }
            else
            {
                //Components
                if (TypeGroupPicker.SelectedIndex == 1)
                {
                    List<IData> results = MauiProgram._context.Components.Where(c => c.Name.Contains(SearchEntry.Text) || c.Description.Contains(SearchEntry.Text)).Select(c => (IData)c).OrderBy(r => r.Name).ToList();
                    App.Current.MainPage = new SearchAndViewPage(results, SearchEntry.Text, TypeGroupPicker.SelectedIndex);
                }
                //Recipes
                else if (TypeGroupPicker.SelectedIndex == 2)
                {
                    List<IData> results = MauiProgram._context.Recipes.Where(c => c.Name.Contains(SearchEntry.Text) || c.Description.Contains(SearchEntry.Text)).Select(r => (IData)r).OrderBy(r => r.Name).ToList();
                    App.Current.MainPage = new SearchAndViewPage(results, SearchEntry.Text, TypeGroupPicker.SelectedIndex);
                }
                //groups
                else
                {
                    List<IData> results = MauiProgram._context.Components.Where(c => c.GroupId == TypeGroupPicker.SelectedIndex - offset && (c.Name.Contains(SearchEntry.Text) || c.Description.Contains(SearchEntry.Text))).Select(component => (IData)component).ToList();
                    results.AddRange(MauiProgram._context.Recipes.Where(c => c.GroupId == TypeGroupPicker.SelectedIndex - offset && (c.Name.Contains(SearchEntry.Text) || c.Description.Contains(SearchEntry.Text))).Select(component => (IData)component).ToList());
                    results = results.OrderBy(r => r.Name).ToList();
                    App.Current.MainPage = new SearchAndViewPage(results, SearchEntry.Text, TypeGroupPicker.SelectedIndex);
                }
            }
        }
    }

    private void OnFrameTapped(int id, Type type)
    {
        // TODO
        Console.WriteLine("Frame wurde angetippt.");
    }

    private async void OnAddGroup(object sender, EventArgs e)
    {
        var popup = new AddGroupPopup();
        var result = (Group?)await this.ShowPopupAsync(popup);
        //TODO
    }

    private async void OnAddComponent(object sender, EventArgs e)
    {
        var popup = new AddComponentPopup();
        var result = (Component?)await this.ShowPopupAsync(popup);
        //TODO
    }

    private async void OnAddRecipe(object sender, EventArgs e)
    {
        var popup = new AddRecipePopup();
        var result = (Recipe?)await this.ShowPopupAsync(popup);
        //TODO
    }



    //doubeled
    private void OnSettings(object sender, EventArgs e)
    {
        //TODO: Change to Popup
        SetLanguage("de");
    }

    private void SetLanguage(string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        ResourceManager rm = AppLanguage.ResourceManager;
        rm.ReleaseAllResources();
        MauiProgram.configuration["DefaultLanguage"] = culture;
        App.Current.MainPage = new SearchAndViewPage();
    }

    private void SearchEntry_TextChanged(object sender, TextChangedEventArgs e)
    {

    }
}