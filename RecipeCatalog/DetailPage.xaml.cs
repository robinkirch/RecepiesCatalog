using CommunityToolkit.Maui.Views;
using RecipeCatalog.Data;
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

    public DetailPage(IData data)
    {
        InitializeComponent();
        DataType = data.GetType();
        ID = data.Id;
        AdminArea.IsVisible = !MauiProgram.IsThisUserAdmin();
        ChangeCommonData(data);

        if (DataType == typeof(Recipe))
            AddComponents(MauiProgram._context.RecipeComponents.Where(rc => rc.RecipeId == data.Id).ToList(), !MauiProgram._context.MissingViewRightsRecipes.Any(m => m.RecipeId == ID && m.UserId == MauiProgram.CurrentUser.Id && m.CannotSeeComponents));

        if (DataType == typeof(Component))
        {
            HR.IsVisible = false;
            AddUsedIn(data.Id);
        }
    }

    public void ChangeCommonData(IData data)
    {
        DetailImage.Source = MauiProgram.ByteArrayToImageSource(data.Image);
        AliasText.Text = (data.Aliases != null && data.Aliases.Length > 0) ? "(" + string.Join(",", data.Aliases) + ")" : string.Empty;
        NameText.Text = data.Name;
        if (!(DataType == typeof(Recipe) && MauiProgram._context.MissingViewRightsRecipes.Any(m => m.RecipeId == ID && m.UserId == MauiProgram.CurrentUser.Id && m.CannotSeeDescription)) &&
            !(DataType == typeof(Component) && MauiProgram._context.MissingViewRightsComponents.Any(m => m.ComponentId == ID && m.UserId == MauiProgram.CurrentUser.Id && m.CannotSeeDescription)))
        {
            DescriptionText.Text = data.Description;
        }
        else
            DescriptionText.Text = "???";
        GroupText.Text = MauiProgram._context.Groups.Single(g => g.Id == data.GroupId).GroupName;
    }

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
                    Text = !rightsToSee.Any(ri => ri.RecipeId == r.Id) ? "???" : r.Name,
                    TextColor = !groups.Any(g => g.GroupId == r.GroupId) ? Color.Parse("DarkGray") : Color.Parse("DarkRed"),
                    VerticalOptions = LayoutOptions.Start
                };
                if (rightsToSee.Any(ri => ri.RecipeId == r.Id) && !groups.Any(g => g.GroupId == r.GroupId))
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

        foreach (var item in data)
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
    /// Handles the tap event on a frame and navigates to the now updated detail page.
    /// </summary>
    /// <param name="page">The detail page to navigate to.</param>
    private static void OnFrameTapped(DetailPage page)
    {
        App.Current!.MainPage = page;
    }
    private void OnBack(object sender, EventArgs e)
    {
        //TODO: Missing searchdata
        App.Current!.MainPage = new SearchAndViewPage();
    }

    private void OnEdit(object sender, EventArgs e)
    {
        IData data = DataType switch
        {
            Type t when t == typeof(Component) => MauiProgram._context.Components.Single(c => c.Id == ID),
            Type t when t == typeof(Recipe) => MauiProgram._context.Recipes.Single(c => c.Id == ID),
            _ => throw new NotImplementedException("type not implemented"),
        };

        ImagePicker.IsVisible = true;
        NameText.Text = AppLanguage.Username;
        NameEntry.IsVisible = true;
        NameEntry.Text = data.Name;
        DescriptionText.Text = AppLanguage.Placeholder_Description;
        DescEntry.IsVisible = true;
        DescEntry.Text = data.Description;
        AliasText.Text = AppLanguage.Alias;
        AliasEntry.IsVisible = true;
        AliasEntry.Text = string.Join(",", data.Aliases ?? []);
        SaveBtn.IsVisible = true;
        LoadPickerData();
        LoadUserData();
        //todo: remove image from componen and recipe popup
    }

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
    private void LoadComponents()
    {
        var componentsForRecipe = MauiProgram._context.RecipeComponents.Where(c => c.RecipeId == ID).ToList();
        var components = new ObservableCollection<ComponentView>();
        MauiProgram._context.Components.ToList().ForEach(c =>
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

    private void LoadUserData()
    {
        //TODO:Optimieren, da components anzeige vollständig in recipes enthalten ist und immer geladen wird
        if (DataType == typeof(Component))
        {
            LabelCompView.IsVisible = true;
            LabelCompDesc.IsVisible = true;
            FrameCompView.IsVisible = true;
            FrameCompDesc.IsVisible = true;
            var missingRightsComp = MauiProgram._context.MissingViewRightsComponents.Where(m => m.ComponentId == ID).ToList();
            var compviewSettings = new ObservableCollection<UserView>();
            var compdescSettings = new ObservableCollection<UserView>();

            MauiProgram._context.Users.ToList().ForEach(u =>
            {
                compviewSettings.Add(new() { UserName = u.Username, Id = u.Id, IsSelected = missingRightsComp.Any(m => m.UserId == u.Id && m.CannotSee) });
                compdescSettings.Add(new() { UserName = u.Username, Id = u.Id, IsSelected = missingRightsComp.Any(m => m.UserId == u.Id && m.CannotSeeDescription) });
            });

            ComponentViewCollectionView.ItemsSource = compviewSettings;
            ComponentDescCollectionView.ItemsSource = compdescSettings;
        }
        else if (DataType == typeof(Recipe))
        {
            LoadComponents();
            EditComponentLabel.IsVisible = true;
            EditComponentFrame.IsVisible = true;

            LabelRecView.IsVisible = true;
            LabelRecDesc.IsVisible = true;
            LabelRecComp.IsVisible = true;
            FrameRecView.IsVisible = true;
            FrameRecDesc.IsVisible = true;
            FrameRecComp.IsVisible = true;
            var missingRightsRec = MauiProgram._context.MissingViewRightsRecipes.Where(m => m.RecipeId == ID).ToList();
            var recviewSettings = new ObservableCollection<UserView>();
            var recdescSettings = new ObservableCollection<UserView>();
            var reccompSettings = new ObservableCollection<UserView>();

            MauiProgram._context.Users.ToList().ForEach(u =>
            {
                recviewSettings.Add(new() { UserName = u.Username, Id = u.Id, IsSelected = missingRightsRec.Any(m => m.UserId == u.Id && m.CannotSee) });
                recdescSettings.Add(new() { UserName = u.Username, Id = u.Id, IsSelected = missingRightsRec.Any(m => m.UserId == u.Id && m.CannotSeeDescription) });
                reccompSettings.Add(new() { UserName = u.Username, Id = u.Id, IsSelected = missingRightsRec.Any(m => m.UserId == u.Id && m.CannotSeeComponents) });
            });

            RecipeViewCollectionView.ItemsSource = recviewSettings;
            RecipeDescCollectionView.ItemsSource = recdescSettings;
            RecipeCompCollectionView.ItemsSource = reccompSettings;
        }
        else 
            throw new NotImplementedException();
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

    private async void OnDelete(object sender, EventArgs e)
    {
        //TOD: Unvollständig
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

    private void OnSave(object sender, EventArgs e)
    {
        IData data = DataType == typeof(Component) ? MauiProgram._context.Components.Single(c => c.Id == ID) : MauiProgram._context.Recipes.Single(c => c.Id == ID);
        
        //Saving the changes
        //Maindata
        data.Image = selectedImage;
        data.Name = NameEntry.Text;
        data.Description = DescEntry.Text;
        data.Aliases = (AliasEntry.Text != null) ? AliasEntry.Text.Split(',') : [];
        data.GroupId = (GroupPicker.SelectedIndex != -1) ? ((Group)GroupPicker.SelectedItem).Id : null;

        //ComponentsUsed
        if (DataType == typeof(Recipe))
        {
            var RecipesComponents = new List<RecipeComponents>();
            foreach (ComponentView item in EditComponentCollectionView.ItemsSource)
            {
                if (item.IsSelected)
                    RecipesComponents.Add(new() { Count = item.Count, ComponentId = item.Id });
            }
            MauiProgram._context.RecipeComponents.RemoveRange(MauiProgram._context.RecipeComponents.Where(c => c.RecipeId == ID).ToList());
            MauiProgram._context.RecipeComponents.AddRange(RecipesComponents);
        }

        //Rights
        if (DataType == typeof(Component))
        {
            List<UserView> descs = (ComponentDescCollectionView.ItemsSource as ObservableCollection<UserView>)!.ToList();
            (ComponentViewCollectionView.ItemsSource as ObservableCollection<UserView>)!.ToList().ForEach(u =>
            {
                bool descSelected = descs.Where(d => d.Id == u.Id).Single().IsSelected;
                var entry = MauiProgram._context.MissingViewRightsComponents.Where(m => m.UserId == u.Id && m.ComponentId == ID).SingleOrDefault();
                if (entry == null && (u.IsSelected || descSelected))
                {
                    entry = new()
                    {
                        UserId = u.Id,
                        ComponentId = ID,
                        CannotSee = u.IsSelected,
                        CannotSeeDescription = descSelected,
                    };
                    MauiProgram._context.MissingViewRightsComponents.Add(entry);
                }
                else if (entry != null)
                {
                    entry.CannotSee = u.IsSelected;
                    entry.CannotSeeDescription = descSelected;
                }
            });
        }
        else if (DataType == typeof(Recipe))
        {
            List<UserView> descs = (RecipeDescCollectionView.ItemsSource as ObservableCollection<UserView>)!.ToList();
            List<UserView> comps = (RecipeCompCollectionView.ItemsSource as ObservableCollection<UserView>)!.ToList();
            (RecipeViewCollectionView.ItemsSource as ObservableCollection<UserView>)!.ToList().ForEach(u =>
            {
                bool descSelected = descs.Where(d => d.Id == u.Id).Single().IsSelected;
                bool compSelected = comps.Where(d => d.Id == u.Id).Single().IsSelected;
                var entry = MauiProgram._context.MissingViewRightsRecipes.Where(m => m.UserId == u.Id && m.RecipeId == ID).SingleOrDefault();
                if (entry == null && (u.IsSelected || descSelected || compSelected))
                {
                    entry = new()
                    {
                        UserId = u.Id,
                        RecipeId = ID,
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
            });
        }

        MauiProgram._context.SaveChanges();
        App.Current!.MainPage = new DetailPage(data);
    }
}