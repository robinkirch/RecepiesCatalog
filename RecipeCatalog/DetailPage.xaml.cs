using CommunityToolkit.Maui.Views;
using RecipeCatalog.Data;
using RecipeCatalog.Helper;
using RecipeCatalog.Models;
using RecipeCatalog.Popups;
using RecipeCatalog.Resources.Language;
using System.Collections.ObjectModel;
using Group = RecipeCatalog.Models.Group;

namespace RecipeCatalog;

public partial class DetailPage : ContentPage
{
    private readonly Type DataType;
    private readonly int ID;
    private byte[]? selectedImage;

    /// <summary>
    /// Initializes a new instance of the <see cref="DetailPage"/> class.
    /// Sets up the detail page with data based on the provided <paramref name="data"/> object.
    /// </summary>
    /// <param name="data">The data object containing details to display on the page.</param>
    public DetailPage(IData data)
    {
        InitializeComponent();
        DataType = data.GetType();
        ID = data.Id;
        AdminArea.IsVisible = MauiProgram.CurrentUser.IsAdmin;
        ChangeCommonData(data);

        if (DataType == typeof(Recipe))
            AddComponents(MauiProgram._context.RecipeComponents.Where(rc => rc.RecipeId == data.Id).ToList(), !MauiProgram._context.MissingViewRightsRecipes.Any(m => m.RecipeId == ID && m.UserId == MauiProgram.CurrentUser.Id && m.CannotSeeComponents));

        if (DataType == typeof(Component))
        {
            HR.IsVisible = false;
            AddUsedIn(data.Id);
        }
    }

    /// <summary>
    /// Updates the common data fields on the detail page with values from the provided <paramref name="data"/> object.
    /// </summary>
    /// <param name="data">The data object containing information to display.</param>
    public void ChangeCommonData(IData data)
    {
        DetailImage.Source = MauiProgram.ByteArrayToImageSource(data.Image);
        selectedImage = data.Image;
        AliasText.Text = (data.Aliases != null && data.Aliases[0].Length > 0) ? "(" + string.Join(",", data.Aliases) + ")" : string.Empty;
        NameText.Text = data.Name;
        if (!(DataType == typeof(Recipe) && MauiProgram._context.MissingViewRightsRecipes.Any(m => m.RecipeId == ID && m.UserId == MauiProgram.CurrentUser.Id && m.CannotSeeDescription)) &&
            !(DataType == typeof(Component) && MauiProgram._context.MissingViewRightsComponents.Any(m => m.ComponentId == ID && m.UserId == MauiProgram.CurrentUser.Id && m.CannotSeeDescription)))
        {
            DescriptionText.Text = data.Description;
        }
        else
            DescriptionText.Text = "???";
        GroupText.Text = MauiProgram._context.Groups.Single(g => g.Id == data.GroupId).GroupName;
        if (MauiProgram.CurrentUser.IsAdmin)
        {
            SecretDescriptionText.Text = data.SecretDescription != null && data.SecretDescription != string.Empty ? $"\'{data.SecretDescription}\'" : string.Empty;
            SecretDescriptionText.IsVisible = true;
        }
            
    }

    /// <summary>
    /// Adds a list of recipes that the specified component is used in to the detail page.
    /// </summary>
    /// <param name="componentId">The ID of the component to check for usage in recipes.</param>
    public void AddUsedIn(int componentId)
    {
        //TODO: scheinbar habe ich die Navigationen in den klassen nicht vernünftig gemacht, weswegen die abfragen so ass sind
        var recipeIds = MauiProgram._context.RecipeComponents
            .Where(rc => rc.ComponentId == componentId)
            .Select(rc => rc.RecipeId)
            .ToList();
        var recipes = MauiProgram._context.Recipes
            .Where(r => recipeIds.Contains(r.Id))
            .ToList();
        var rightsToSee = MauiProgram._context.MissingViewRightsRecipes
            .Where(m => recipeIds.Contains(m.RecipeId)
                         && m.UserId == MauiProgram.CurrentUser.Id
                         && !(m.CannotSee || m.CannotSeeComponents))
            .ToList();
        var groups = MauiProgram._context.MissingViewRightsGroups.Where(m => m.UserId == MauiProgram.CurrentUser.Id && recipes.Select(r => r.GroupId).Contains(m.GroupId)).ToList();

        if (recipes.Count > 0)
        {
            HR.IsVisible = true;
            Recipes.IsVisible = true;
            int row = 0;

            recipes.ForEach(r =>
            {
                RecipesComponentsPlace.RowDefinitions.Add(new RowDefinition { Height = 20 });
                var frame = new Frame
                {
                    Padding = 0,
                    Margin = 0,
                    HasShadow = false,
                    BackgroundColor = Color.FromArgb("#50D3D3D3"),
                };

                var nameLabel = new Label
                {
                    Text = !rightsToSee.Any(ri => ri.RecipeId == r.Id) ? r.Name : "???",
                    TextColor = !groups.Any(g => g.GroupId == r.GroupId) ? Color.Parse("DarkGray") : Color.Parse("DarkRed"),
                    VerticalOptions = LayoutOptions.Start
                };
                if (!rightsToSee.Any(ri => ri.RecipeId == r.Id) && !groups.Any(g => g.GroupId == r.GroupId))
                {
                    var tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += (s, e) =>
                    {
                        OnFrameTapped(new DetailPage(recipes.Single(re => r.Id == re.Id)));
                    };
                    nameLabel.GestureRecognizers.Add(tapGestureRecognizer);
                }
                frame.Content = nameLabel;

                RecipesComponentsPlace.Children.Add(nameLabel);
                Grid.SetRow(nameLabel, row);
                Grid.SetColumn(nameLabel, 0);
                row++;
            });
        }
    }

    /// <summary>
    /// Adds a list of components used in the specified recipe to the detail page.
    /// </summary>
    /// <param name="data">The list of recipe components to add.</param>
    /// <param name="isAccessible">Indicates whether the components are accessible to the current user or should be displayed different.</param>
    public void AddComponents(List<RecipeComponents> data, bool isAccessible)
    {
        int row = 0;
        if (data.Count != 0)
        {
            Components.IsVisible = true;
            RecipesComponentsPlace.RowDefinitions.Add(new RowDefinition { Height = 20 });

            //head
            var componentLabel = new Label
            {
                Text = AppLanguage.TableComponents,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15,
                VerticalOptions = LayoutOptions.Start
            };
            RecipesComponentsPlace.Children.Add(componentLabel);
            Grid.SetRow(componentLabel, row);
            Grid.SetColumn(componentLabel, 0);
            var counterLabel = new Label
            {
                Text = AppLanguage.TableCount,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15,
                VerticalOptions = LayoutOptions.Start
            };
            RecipesComponentsPlace.Children.Add(counterLabel);
            Grid.SetRow(counterLabel, row);
            Grid.SetColumn(counterLabel, 2);

            row++;
        }

        foreach (var item in data) //Todo: maybe order by
        {
            RecipesComponentsPlace.RowDefinitions.Add(new RowDefinition { Height = 20 });
            var component = MauiProgram._context.Components.Where(c => c.Id == item.ComponentId).Single();
            bool groupBlocked = MauiProgram._context.MissingViewRightsGroups.Any(m => m.UserId == MauiProgram.CurrentUser.Id && component.GroupId == m.GroupId);
            var frame = new Frame
            {
                Padding = 0,
                Margin = 0,
                HasShadow = false,
                BackgroundColor = Color.FromArgb("#50D3D3D3"),
            };
            var nameLabel = new Label
            {
                Text = isAccessible ? component.Name : "???",
                TextColor = !groupBlocked ? Color.Parse("DarkGray") : Color.Parse("DarkRed"),
                VerticalOptions = LayoutOptions.Start
            };

            if (isAccessible && !groupBlocked)
            {
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) =>
                {
                    OnFrameTapped(new DetailPage(component));
                };
                nameLabel.GestureRecognizers.Add(tapGestureRecognizer);
            }
            frame.Content = nameLabel;

            RecipesComponentsPlace.Children.Add(nameLabel);
            Grid.SetRow(nameLabel, row);
            Grid.SetColumn(nameLabel, 0);

            var countLabel = new Label
            {

                Text = isAccessible ? item.Count.ToString() : "?",
                VerticalOptions = LayoutOptions.Center
            };
            RecipesComponentsPlace.Children.Add(countLabel);
            Grid.SetRow(countLabel, row);
            Grid.SetColumn(countLabel, 2);

            row++;
        }
    }

    /// <summary>
    /// Handles the tap event on a frame and navigates to the specified detail page.
    /// </summary>
    /// <param name="page">The detail page to navigate to.</param>
    private static void OnFrameTapped(DetailPage page)
    {
        App.Current!.MainPage = page;
    }

    /// <summary>
    /// Handles the event when the back button is clicked and navigates to the search and view page.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void OnBack(object sender, EventArgs e)
    {
        //TODO: Missing searchdata
        App.Current!.MainPage = new SearchAndViewPage();
    }

    /// <summary>
    /// Handles the event when the edit button is clicked, displaying the editable fields for the data.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void OnEdit(object sender, EventArgs e)
    {
        IData data = DataType switch
        {
            Type t when t == typeof(Component) => MauiProgram._context.Components.Single(c => c.Id == ID),
            Type t when t == typeof(Recipe) => MauiProgram._context.Recipes.Single(c => c.Id == ID),
            _ => throw new NotImplementedException("type not implemented"),
        };

        EditBtn.IsVisible = false;
        ImagePicker.IsVisible = true;
        NameText.Text = AppLanguage.Username;
        NameEntry.IsVisible = true;
        NameEntry.Text = data.Name;
        DescriptionText.Text = AppLanguage.Placeholder_Description;
        DescEntry.IsVisible = true;
        DescEntry.Text = data.Description;
        SecretDescriptionText.Text = AppLanguage.Placeholder_SecretDescription;
        SecretDescEntry.IsVisible = true;
        SecretDescEntry.Text = data.SecretDescription;
        AliasText.Text = AppLanguage.Alias;
        AliasEntry.IsVisible = true;
        AliasEntry.Text = string.Join(",", data.Aliases ?? []);
        SaveBtn.IsVisible = true;
        LoadPickerData();
        LoadUserData();
        //todo: remove image from componen and recipe popup
    }

    /// <summary>
    /// Loads the available groups into the group picker and selects the current group if it exists.
    /// </summary>
    private void LoadPickerData()
    {
        GroupPicker.ItemsSource = MauiProgram._context.Groups.ToList();
        GroupPicker.ItemDisplayBinding = new Binding("GroupName");
        GroupText.Text = AppLanguage.Placeholder_Groups;
        GroupPicker.IsVisible = true;

        int? selectedGroupId = DataType == typeof(Component) ? MauiProgram._context.Components.Single(c => c.Id == ID).GroupId : MauiProgram._context.Recipes.Single(c => c.Id == ID).GroupId;

        if (selectedGroupId != null)
        {
            var selectedGroup = MauiProgram._context.Groups.FirstOrDefault(g => g.Id == selectedGroupId);

            if(selectedGroup != null)
                GroupPicker.SelectedItem = selectedGroup;
            else
                GroupPicker.SelectedIndex = -1;
        }
        else
            GroupPicker.SelectedIndex = -1;
    }

    /// <summary>
    /// Loads the components used in the current recipe into a collection view for editing.
    /// </summary>
    private void LoadComponents()
    {
        var componentsForRecipe = MauiProgram._context.RecipeComponents.Where(c => c.RecipeId == ID).ToList();
        var components = new ObservableCollection<ComponentView>();
        MauiProgram._context.Components.OrderBy(c => c.Name).ToList().ForEach(c =>
        {
            components.Add(new() { 
                Name = c.Name, 
                Id = c.Id, 
                IsSelected = componentsForRecipe.Any(co => co.ComponentId == c.Id), 
                Count = componentsForRecipe.Any(co => co.ComponentId == c.Id) ? componentsForRecipe.Single(co => co.ComponentId == c.Id).Count : 0,
            });
        });
        EditComponentCollectionView.ItemsSource = components;
    }

    /// <summary>
    /// Loads the user-specific visibility settings for the component or recipe into collection views.
    /// This includes settings for view and description access for the current user.
    /// </summary>
    private void LoadUserData()
    {
        //TODO:Optimieren, da components anzeige vollständig in recipes enthalten ist und immer geladen wird
        if (DataType == typeof(Component))
        {
            var missingRightsComp = MauiProgram._context.MissingViewRightsComponents.Where(m => m.ComponentId == ID).ToList();
            var userItems = new ObservableCollection<MissingViewRightComponentUserItem>();
            MauiProgram._context.Users.ToList().ForEach(u =>
            {
                userItems.Add(new MissingViewRightComponentUserItem { UserID = u.Id, UserName = u.Username, CannotSee = missingRightsComp.Any(m => m.UserId == u.Id && m.CannotSee), CannotSeeDescription = missingRightsComp.Any(m => m.UserId == u.Id && m.CannotSeeDescription) });
            });
            DynamicTableControlRightsComponent.ItemsSource = new ObservableCollection<object>(userItems.Cast<object>());
            DynamicTableControlRightsComponent.BuildTable(AppLanguage.User_CustomRights);
            DynamicTableControlRightsComponent.IsVisible = true;
        }
        else if (DataType == typeof(Recipe))
        {
            LoadComponents();
            EditComponentLabel.IsVisible = true;
            EditComponentFrame.IsVisible = true;


            var missingRightsRec = MauiProgram._context.MissingViewRightsRecipes.Where(m => m.RecipeId == ID).ToList();
            var userItems = new ObservableCollection<MissingViewRightRecipeUserItem>();
            MauiProgram._context.Users.ToList().ForEach(u =>
            {
                userItems.Add(new MissingViewRightRecipeUserItem { UserID = u.Id, UserName = u.Username, CannotSee = missingRightsRec.Any(m => m.UserId == u.Id && m.CannotSee), CannotSeeDescription = missingRightsRec.Any(m => m.UserId == u.Id && m.CannotSeeDescription), CannotSeeComponents = missingRightsRec.Any(m => m.UserId == u.Id && m.CannotSeeComponents) });
            });
            DynamicTableControlRightsRecipe.ItemsSource = new ObservableCollection<object>(userItems.Cast<object>());
            DynamicTableControlRightsRecipe.BuildTable(AppLanguage.User_CustomRights);
            DynamicTableControlRightsRecipe.IsVisible = true;
        }
        else 
            throw new NotImplementedException();
    }

    /// <summary>
    /// Handles the event when an image is selected from the file picker. 
    /// This method updates the selected image byte array with the image chosen by the user.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event arguments.</param>
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
        DetailImage.Source = MauiProgram.ByteArrayToImageSource(selectedImage);
    }

    /// <summary>
    /// Handles the event when the delete button is clicked. 
    /// Displays a confirmation popup and deletes the component or recipe if confirmed.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnDelete(object sender, EventArgs e)
    {
        //TODO: Unvollständig
        var popup = new DeletePopup();
        var result = (bool?)await this.ShowPopupAsync(popup);
        if (result == null || result == false)
            return;

        switch (DataType)
        {
            case Type t when t == typeof(Component):
                List<string> recipes = MauiProgram._context.RecipeComponents.Where(rc => rc.ComponentId == ID).Select(rc => MauiProgram._context.Recipes.Where(r => r.Id == rc.RecipeId).Select(r => r.Name).Single()).ToList();
                if (recipes.Count == 0)
                {
                    MauiProgram._context.Components.Remove(MauiProgram._context.Components.Single(c => c.Id == ID));
                    MauiProgram._context.SaveChanges();
                    //TODO: Missing searchdata
                    App.Current!.MainPage = new SearchAndViewPage();
                }
                else
                {
                    var error = new DeleteErrorPopup(recipes);
                    var res = (bool?)await this.ShowPopupAsync(error);
                    if (res != null && res == true)
                    {
                        MauiProgram._context.RecipeComponents.Where(rc => rc.ComponentId == ID).ToList().ForEach(rc =>
                        {
                            MauiProgram._context.RecipeComponents.Remove(rc);
                        });
                        MauiProgram._context.SaveChanges();
                        MauiProgram._context.Components.Remove(MauiProgram._context.Components.Single(c => c.Id == ID));
                        MauiProgram._context.SaveChanges();
                        //TODO: Missing searchdata
                        App.Current!.MainPage = new SearchAndViewPage();
                    }
                }
                break;
            case Type t when t == typeof(Recipe):
                MauiProgram._context.RecipeComponents.Where(rc => rc.RecipeId == ID).ToList().ForEach(rc =>
                {
                    MauiProgram._context.RecipeComponents.Remove(rc);
                });
                MauiProgram._context.SaveChanges();
                MauiProgram._context.Recipes.Remove(MauiProgram._context.Recipes.Single(c => c.Id == ID));
                MauiProgram._context.SaveChanges();
                //TODO: Missing searchdata
                App.Current!.MainPage = new SearchAndViewPage();
                break;
            default:
                throw new NotImplementedException("type not implemented");
        }
    }

    /// <summary>
    /// Handles the event when the save button is clicked. 
    /// Updates the component or recipe data with the values from the entry fields 
    /// and saves the changes to the database.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void OnSave(object sender, EventArgs e)
    {
        IData data = DataType == typeof(Component) ? MauiProgram._context.Components.Single(c => c.Id == ID) : MauiProgram._context.Recipes.Single(c => c.Id == ID);
        
        //Maindata
        data.Image = selectedImage;
        data.Name = NameEntry.Text;
        data.Description = DescEntry.Text;
        data.SecretDescription = SecretDescEntry.Text;
        data.Aliases = (AliasEntry.Text != null) ? AliasEntry.Text.Split(',') : [];
        data.GroupId = (GroupPicker.SelectedIndex != -1) ? ((Group)GroupPicker.SelectedItem).Id : null;

        //ComponentsUsed
        if (DataType == typeof(Recipe))
        {
            var RecipesComponents = new List<RecipeComponents>();
            foreach (ComponentView item in EditComponentCollectionView.ItemsSource)
            {
                if (item.IsSelected)
                    RecipesComponents.Add(new() { Count = item.Count, ComponentId = item.Id, RecipeId = data.Id });
            }
            MauiProgram._context.RecipeComponents.RemoveRange(MauiProgram._context.RecipeComponents.Where(c => c.RecipeId == ID).ToList());
            MauiProgram._context.RecipeComponents.AddRange(RecipesComponents);
        }

        //Rights
        if (DataType == typeof(Component))
        {
            DynamicTableControlRightsComponent.ItemsSource.ToList().ForEach(item =>
            {
                if (item is MissingViewRightComponentUserItem c) // Pattern Matching is neccessary
                {
                    var entry = MauiProgram._context.MissingViewRightsComponents.Where(m => m.UserId == c.UserID && m.ComponentId == ID).SingleOrDefault();
                    if (entry == null && (c.CannotSee || c.CannotSeeDescription))
                    {
                        entry = new()
                        {
                            UserId = c.UserID,
                            ComponentId = ID,
                            CannotSee = c.CannotSee,
                            CannotSeeDescription = c.CannotSeeDescription,
                        };
                        MauiProgram._context.MissingViewRightsComponents.Add(entry);
                    }
                    else if (entry != null)
                    {
                        entry.CannotSee = c.CannotSee;
                        entry.CannotSeeDescription = c.CannotSeeDescription;
                    }
                }
            });
        }
        else if (DataType == typeof(Recipe))
        {
            DynamicTableControlRightsRecipe.ItemsSource.ToList().ForEach(item =>
            {
                if (item is MissingViewRightRecipeUserItem c) // Pattern Matching is neccessary
                {
                    var entry = MauiProgram._context.MissingViewRightsRecipes.Where(m => m.UserId == c.UserID && m.RecipeId == ID).SingleOrDefault();
                    if (entry == null && (c.CannotSee || c.CannotSeeDescription || c.CannotSeeComponents))
                    {
                        entry = new()
                        {
                            UserId = c.UserID,
                            RecipeId = ID,
                            CannotSee = c.CannotSee,
                            CannotSeeDescription = c.CannotSeeDescription,
                            CannotSeeComponents = c.CannotSeeComponents,
                        };
                        MauiProgram._context.MissingViewRightsRecipes.Add(entry);
                    }
                    else if (entry != null)
                    {
                        entry.CannotSee = c.CannotSee;
                        entry.CannotSeeDescription = c.CannotSeeDescription;
                        entry.CannotSeeComponents = c.CannotSeeComponents;
                    }
                }
            });
        }

        MauiProgram._context.SaveChanges();
        App.Current!.MainPage = new DetailPage(data);
    }
}