using CommunityToolkit.Maui.Views;
using RecipeCatalog.Data;
using RecipeCatalog.Helper;
using RecipeCatalog.Models;
using RecipeCatalog.Popups;
using RecipeCatalog.Resources.Language;
using System.Collections.ObjectModel;
using Category = RecipeCatalog.Models.Category;

namespace RecipeCatalog;

public partial class DetailPage : ContentPage
{
    private readonly Type DataType;
    private readonly int ID;
    private byte[]? selectedImage;

    /// <summary>
    /// Initializes a new instance of the <see cref="DetailPage"/> class. It sets up the page to display details for the provided <paramref name="data"/> object.
    /// If the data type is a Recipe or Component, it adjusts the page accordingly by adding components or associated recipes.
    /// </summary>
    /// <param name="data">The data object (either Recipe or Component) that contains the details to display on the page.</param>
    public DetailPage(IData data)
    {
        InitializeComponent();
        DataType = data.GetType();
        ID = data.Id;
        AdminArea.IsVisible = MauiProgram.CurrentUser.IsAdmin;
        ChangeCommonData(data);

        if (DataType == typeof(Recipe))
        {
            AddComponents(MauiProgram._context.RecipeComponents.Where(rc => rc.RecipeId != null && rc.RecipeId == ID).ToList(), !MauiProgram._context.MissingViewRightsRecipes.Any(m => m.RecipeId == ID && m.UserId == MauiProgram.CurrentUser.Id && m.CannotSeeComponents));
            HR.IsVisible = false;
            AddUsedInForRecipes();
        }
        if (DataType == typeof(Component))
        {
            HR.IsVisible = false;
            AddUsedInForComponents();
        }
    }

    /// <summary>
    /// Updates common fields on the page such as the image, alias, name, description, and category based on the provided <paramref name="data"/> object.
    /// The description is shown unless the user doesn't have the rights to view it (in which case "???" is displayed).
    /// If the current user is an admin, it shows a secret description if available.
    /// </summary>
    /// <param name="data">The data object (either Recipe or Component) containing the information to be displayed on the page.</param>
    public void ChangeCommonData(IData data)
    {
        DetailImage.Source = MauiProgram.ByteArrayToImageSource(data.Image);
        selectedImage = data.Image;
        AliasText.Text = (data.Aliases != null && data.Aliases.Length > 0 && data.Aliases[0].Length > 0) ? "(" + string.Join(",", data.Aliases) + ")" : string.Empty;
        NameText.Text = data.Name;
        if (!(DataType == typeof(Recipe) && MauiProgram._context.MissingViewRightsRecipes.Any(m => m.RecipeId == ID && m.UserId == MauiProgram.CurrentUser.Id && m.CannotSeeDescription)) &&
            !(DataType == typeof(Component) && MauiProgram._context.MissingViewRightsComponents.Any(m => m.ComponentId == ID && m.UserId == MauiProgram.CurrentUser.Id && m.CannotSeeDescription)))
        {
            DescriptionText.Text = data.Description;
        }
        else
            DescriptionText.Text = "???";
        CategoryText.Text = MauiProgram._context.Categories.Single(g => g.Id == data.CategoryId).CategoryName;
        if (MauiProgram.CurrentUser.IsAdmin)
        {
            SecretDescriptionText.Text = data.SecretDescription != null && data.SecretDescription != string.Empty ? $"\'{data.SecretDescription}\'" : string.Empty;
            SecretDescriptionText.IsVisible = true;
        }
            
    }

    /// <summary>
    /// Retrieves the recipes that use the current recipe (referenced in RecipeComponents) and adds them to the page.
    /// It also checks for user rights to see those recipes, ensuring that the user can view components associated with the recipe.
    /// </summary>
    public void AddUsedInForRecipes()
    {
        //TODO: navigation in models are kinda shitty, so here we are
        var recipeIds = MauiProgram._context.RecipeComponents
           .Where(rc => rc.UsedRecipeId == ID)
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
        AddUsedIn(recipes, rightsToSee);
    }

    /// <summary>
    /// Retrieves the recipes that use the current component (referenced in RecipeComponents) and adds them to the page.
    /// It also checks for user rights to see those recipes, ensuring that the user can view recipes that use the component.
    /// </summary>
    public void AddUsedInForComponents()
    {
        //TODO: navigation in models are kinda shitty, so here we are
        var recipeIds = MauiProgram._context.RecipeComponents
           .Where(rc => rc.ComponentId == ID)
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
        AddUsedIn(recipes, rightsToSee);
    }

    /// <summary>
    /// Adds the recipes that use the current item (either Recipe or Component) to the page.
    /// It ensures that recipes the user doesn't have permission to view are handled correctly by displaying "???" for restricted recipes.
    /// Additionally, it adjusts the styling of the recipe names based on category visibility and user permissions.
    /// </summary>
    /// <param name="recipes">A list of recipes that use the current item (Recipe or Component).</param>
    /// <param name="rightsToSee">A list of MissingViewRightRecipe objects that define the recipes the user has restricted access to.</param>
    public void AddUsedIn(List<Recipe> recipes, List<MissingViewRightRecipe> rightsToSee)
    {
        var categories = MauiProgram._context.MissingViewRightsCategories.Where(m => m.UserId == MauiProgram.CurrentUser.Id && recipes.Select(r => r.CategoryId).Contains(m.CategoryId)).ToList();

        if (recipes.Count > 0)
        {
            HR.IsVisible = true;
            Recipes.IsVisible = true;
            int row = 0;

            recipes.ForEach(r =>
            {
                RecipesPlace.RowDefinitions.Add(new RowDefinition { Height = 20 });
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
                    TextColor = !categories.Any(g => g.CategoryId == r.CategoryId) ? Color.Parse("DarkGray") : Color.Parse("DarkRed"),
                    VerticalOptions = LayoutOptions.Start
                };
                if (!rightsToSee.Any(ri => ri.RecipeId == r.Id) && !categories.Any(g => g.CategoryId == r.CategoryId))
                {
                    var tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += (s, e) =>
                    {
                        OnFrameTapped(new DetailPage(recipes.Single(re => r.Id == re.Id)));
                    };
                    nameLabel.GestureRecognizers.Add(tapGestureRecognizer);
                }
                frame.Content = nameLabel;

                RecipesPlace.Children.Add(nameLabel);
                Grid.SetRow(nameLabel, row);
                Grid.SetColumn(nameLabel, 0);
                row++;
            });
        }
    }

    /// <summary>
    /// Adds a list of components used in the specified recipe to the detail page. It checks whether the components are accessible to the current user, displaying either the component's name or a placeholder ("???") for restricted components.
    /// </summary>
    /// <param name="data">The list of recipe components to add to the page.</param>
    /// <param name="isAccessible">Indicates whether the components are accessible to the current user. If false, the component details will be hidden or replaced with placeholders.</param>
    public void AddComponents(List<RecipeComponents> data, bool isAccessible)
    {
        int row = 0;
        if (data.Count != 0)
        {
            Components.IsVisible = true;
            ComponentsPlace.RowDefinitions.Add(new RowDefinition { Height = 20 });

            //head
            var componentLabel = new Label
            {
                Text = AppLanguage.TableComponents,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15,
                VerticalOptions = LayoutOptions.Start
            };
            ComponentsPlace.Children.Add(componentLabel);
            Grid.SetRow(componentLabel, row);
            Grid.SetColumn(componentLabel, 0);
            var counterLabel = new Label
            {
                Text = AppLanguage.TableCount,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15,
                VerticalOptions = LayoutOptions.Start
            };
            ComponentsPlace.Children.Add(counterLabel);
            Grid.SetRow(counterLabel, row);
            Grid.SetColumn(counterLabel, 2);

            row++;
        }

        foreach (var item in data) //Todo: maybe order by
        {
            ComponentsPlace.RowDefinitions.Add(new RowDefinition { Height = 20 });
            IData component = null;
            if(item.ComponentId != null)
                component = MauiProgram._context.Components.Where(c => c.Id == item.ComponentId).Single();
            else if(item.UsedRecipeId != null)
                component = MauiProgram._context.Recipes.Where(c => c.Id == item.UsedRecipeId).Single();
            else
                throw new NotImplementedException();

            bool categorieBlocked = MauiProgram._context.MissingViewRightsCategories.Any(m => m.UserId == MauiProgram.CurrentUser.Id && component.CategoryId == m.CategoryId);
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
                TextColor = !categorieBlocked ? Color.Parse("DarkGray") : Color.Parse("DarkRed"),
                VerticalOptions = LayoutOptions.Start
            };

            if (isAccessible && !categorieBlocked)
            {
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) =>
                {
                    if (item.ComponentId != null)
                        OnFrameTapped(new DetailPage((Component)component));
                    else if (item.UsedRecipeId != null)
                        OnFrameTapped(new DetailPage((Recipe)component));
                    else
                        throw new NotImplementedException();
                };
                nameLabel.GestureRecognizers.Add(tapGestureRecognizer);
            }
            frame.Content = nameLabel;

            ComponentsPlace.Children.Add(nameLabel);
            Grid.SetRow(nameLabel, row);
            Grid.SetColumn(nameLabel, 0);

            var countLabel = new Label
            {

                Text = isAccessible ? item.Count.ToString() : "?",
                VerticalOptions = LayoutOptions.Center
            };
            ComponentsPlace.Children.Add(countLabel);
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
        try
        {
            App.Current!.MainPage = page;
        }
        catch (Exception ex)
        {
            //TODO: DoLater
        }
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
    }

    /// <summary>
    /// Loads the available categories into the category picker and selects the current category if it exists.
    /// </summary>
    private void LoadPickerData()
    {
        CategoryPicker.ItemsSource = MauiProgram._context.Categories.ToList();
        CategoryPicker.ItemDisplayBinding = new Binding(nameof(Category.CategoryName));
        CategoryText.Text = AppLanguage.Placeholder_Category;
        CategoryPicker.IsVisible = true;

        int? selectedCategorieId = DataType == typeof(Component) ? MauiProgram._context.Components.Single(c => c.Id == ID).CategoryId : MauiProgram._context.Recipes.Single(c => c.Id == ID).CategoryId;

        if (selectedCategorieId != null)
        {
            var selectedCategorie = MauiProgram._context.Categories.FirstOrDefault(g => g.Id == selectedCategorieId);

            if(selectedCategorie != null)
                CategoryPicker.SelectedItem = selectedCategorie;
            else
                CategoryPicker.SelectedIndex = -1;
        }
        else
            CategoryPicker.SelectedIndex = -1;
    }

    /// <summary>
    /// Loads the user-specific visibility settings for the component or recipe into collection views.
    /// This includes settings for view and description access for the current user.
    /// </summary>
    private void LoadUserData()
    {
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
            var missingRightsRec = MauiProgram._context.MissingViewRightsRecipes.Where(m => m.RecipeId == ID).ToList();
            var userItems = new ObservableCollection<MissingViewRightRecipeUserItem>();
            MauiProgram._context.Users.ToList().ForEach(u =>
            {
                userItems.Add(new MissingViewRightRecipeUserItem { UserID = u.Id, UserName = u.Username, CannotSee = missingRightsRec.Any(m => m.UserId == u.Id && m.CannotSee), CannotSeeDescription = missingRightsRec.Any(m => m.UserId == u.Id && m.CannotSeeDescription), CannotSeeComponents = missingRightsRec.Any(m => m.UserId == u.Id && m.CannotSeeComponents) });
            });
            DynamicTableControlRightsRecipe.ItemsSource = new ObservableCollection<object>(userItems.Cast<object>());
            DynamicTableControlRightsRecipe.BuildTable(AppLanguage.User_CustomRights);
            DynamicTableControlRightsRecipe.IsVisible = true;



            List<RecipeComponents> componentRecipes = MauiProgram._context.RecipeComponents.Where(rc => rc.RecipeId == ID).ToList();
            var componentItem = new ObservableCollection<ComponentItem>();
            MauiProgram._context.Components.OrderBy(c => c.Name).ToList().ForEach(c =>
            {
                var d = componentRecipes?.Where(rc => rc.ComponentId == c.Id).SingleOrDefault()?.Count;
                componentItem.Add(new ComponentItem { ID = c.Id, ComponentName = c.Name, Quantity = d ?? 0 });
            });
            DynamicTableControlEditUsedComponent.ItemsSource = new ObservableCollection<object>(componentItem.Cast<object>());
            DynamicTableControlEditUsedComponent.BuildTable(AppLanguage.Filter_Components);
            DynamicTableControlEditUsedComponent.IsVisible = true;


            var recipeItem = new ObservableCollection<RecipeItem>();
            MauiProgram._context.Recipes.OrderBy(c => c.Name).ToList().ForEach(c =>
            {
                var d = componentRecipes?.Where(rc => rc.UsedRecipeId == c.Id).SingleOrDefault()?.Count;
                recipeItem.Add(new RecipeItem { ID = c.Id, RecipeName = c.Name, Quantity = d ?? 0 });
            });
            DynamicTableControlEditUsedRecipe.ItemsSource = new ObservableCollection<object>(recipeItem.Cast<object>());
            DynamicTableControlEditUsedRecipe.BuildTable(AppLanguage.Filter_Recipes);
            DynamicTableControlEditUsedRecipe.IsVisible = true;
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

        data.Image = selectedImage;
        data.Name = NameEntry.Text;
        data.Description = DescEntry.Text;
        data.SecretDescription = SecretDescEntry.Text;
        data.Aliases = (AliasEntry.Text != null) ? AliasEntry.Text.Split(',') : [];
        data.CategoryId = (CategoryPicker.SelectedIndex != -1) ? ((Category)CategoryPicker.SelectedItem).Id : null;

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



            DynamicTableControlEditUsedComponent.ItemsSource.ToList().ForEach(item =>
            {
                if (item is ComponentItem c) // Pattern Matching is neccessary
                {
                    var entry = MauiProgram._context.RecipeComponents.Where(m => m.RecipeId == ID && m.ComponentId == c.ID).SingleOrDefault();
                    if (entry == null && c.Quantity > 0)
                    {
                        entry = new()
                        {
                            RecipeId = ID,
                            ComponentId = c.ID,
                            Count = c.Quantity
                        };
                        MauiProgram._context.RecipeComponents.Add(entry);
                    }
                    else if (entry != null)
                    {
                        if (c.Quantity == 0)
                        {
                            MauiProgram._context.RecipeComponents.Remove(entry);
                        }
                        else
                        {
                            entry.Count = c.Quantity;
                        }
                    }
                }
            });

            DynamicTableControlEditUsedRecipe.ItemsSource.ToList().ForEach(item =>
            {
                if (item is RecipeItem c) // Pattern Matching is neccessary
                {
                    var entry = MauiProgram._context.RecipeComponents.Where(m => m.RecipeId == ID && m.UsedRecipeId == c.ID).SingleOrDefault();
                    if (entry == null && c.Quantity > 0)
                    {
                        entry = new()
                        {
                            RecipeId = ID,
                            UsedRecipeId = c.ID,
                            Count = c.Quantity
                        };
                        MauiProgram._context.RecipeComponents.Add(entry);
                    }
                    else if (entry != null)
                    {
                        if (c.Quantity == 0)
                        {
                            MauiProgram._context.RecipeComponents.Remove(entry);
                        }
                        else
                        {
                            entry.Count = c.Quantity;
                        }
                    }
                }
            });
        }

        MauiProgram._context.SaveChanges();
        App.Current!.MainPage = new DetailPage(data);
    }
}