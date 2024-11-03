using RecipeCatalog.Models;
using RecipeCatalog.Resources.Language;
using System.Collections.ObjectModel;
using RecipeCatalog.Helper;

namespace RecipeCatalog;

public partial class UserRightPage : ContentPage
{
    private readonly User _user;

    public UserRightPage(User user)
    {
        _user = user;
        InitializeComponent();
        LoadData();
        LoadPickerData();

        if (_user != MauiProgram.CurrentUser && MauiProgram.CurrentUser.IsAdmin && _user != MauiProgram.CurrentUser)
        {
            DeleteButton.IsVisible = true;
            PromoteButton.IsVisible = true;
        }

        if (_user.IsAdmin)
            PromoteButton.Text = AppLanguage.DemoteAdmin;
    }

    public void LoadData()
    {
        LabelNameEntry.Text = AppLanguage.Username + ": " + _user.Username;
        NameEntry.Text = _user.Username;
        GuidEntry.Text = _user.Id.ToString();

        var rejectedGroups = MauiProgram._context.MissingViewRightsGroups.Where(m => m.UserId == _user.Id).Select(m => m.GroupId).ToList();
        var groupItems = new ObservableCollection<MissingViewRightGroupItem>();
        MauiProgram._context.Groups.ToList().ForEach(g =>
        {
            groupItems.Add(new MissingViewRightGroupItem { ID = g.Id, GroupName = g.GroupName, CannotAccess = rejectedGroups.Contains(g.Id) });
        });
        DynamicTableControlGroup.ItemsSource = new ObservableCollection<object>(groupItems.Cast<object>());
        DynamicTableControlGroup.BuildTable(AppLanguage.User_GrantedAccessGroups);


        var componentItems = new ObservableCollection<MissingViewRightComponentItem>();
        MauiProgram._context.MissingViewRightsComponents.Where(m => m.UserId == _user.Id).ToList().ForEach(c =>
        {
            componentItems.Add(new MissingViewRightComponentItem { ID = c.Id, ComponentName = c.Component.Name, CannotSee = c.CannotSee, CannotSeeDescription = c.CannotSeeDescription });
        });
        DynamicTableControlComponent.ItemsSource = new ObservableCollection<object>(componentItems.Cast<object>());
        DynamicTableControlComponent.BuildTable(AppLanguage.User_CustomRights + ": " + AppLanguage.Filter_Components);



        var recipeItems = new ObservableCollection<MissingViewRightRecipeItem>();
        MauiProgram._context.MissingViewRightsRecipes.Where(m => m.UserId == _user.Id).ToList().ForEach(c =>
        {
            recipeItems.Add(new MissingViewRightRecipeItem { ID = c.Id, RecipeName = c.Recipe.Name, CannotSee = c.CannotSee, CannotSeeDescription = c.CannotSeeDescription, CannotSeeComponents = c.CannotSeeComponents });
        });
        DynamicTableControlRecipe.ItemsSource = new ObservableCollection<object>(recipeItems.Cast<object>());
        DynamicTableControlRecipe.BuildTable(AppLanguage.User_CustomRights + ": " + AppLanguage.Filter_Recipes);
    }

    private void LoadPickerData()
    {
        var campaigns = MauiProgram._context.Campaigns.ToList();
        CampaignPicker.ItemsSource = campaigns;
        CampaignPicker.ItemDisplayBinding = new Binding("Name");
        if(_user.CampaignId != null)
            CampaignPicker.SelectedItem = campaigns.Where(c => c.Id == _user.CampaignId).Single();
    }


    private void OnSendButtonClicked(object sender, EventArgs e)
    {
        _user.Username = NameEntry.Text;
        if(CampaignPicker.SelectedIndex != -1)
            _user.CampaignId = (CampaignPicker.SelectedItem as Campaign)!.Id;
        MauiProgram._context.Update(_user);

        //groups
        DynamicTableControlGroup.ItemsSource.ToList().ForEach(item =>
        {
            var groups = MauiProgram._context.MissingViewRightsGroups.Where(g => g.UserId == _user.Id).ToList();
            if (item is MissingViewRightGroupItem g) // Pattern Matching is neccessary
            {
                if (g.CannotAccess)
                {
                    if (!groups.Any(gr => gr.GroupId == g.ID))
                        MauiProgram._context.MissingViewRightsGroups.Add(new() { GroupId = g.ID, UserId = _user.Id });
                }
                else
                {
                    if (groups.Any(gr => gr.Id == g.ID))
                        MauiProgram._context.MissingViewRightsGroups.Remove(groups.Single(gr => gr.Id == g.ID));
                }
            }
        });
        MauiProgram._context.SaveChanges();

        //Components
        DynamicTableControlComponent.ItemsSource.ToList().ForEach(item =>
        {
            var components = MauiProgram._context.MissingViewRightsComponents.Where(c => c.UserId == _user.Id).ToList();
            if (item is MissingViewRightComponentItem c) // Pattern Matching is neccessary
            {
                if(c.CannotSee || c.CannotSeeDescription)
                {
                    var comp = components.SingleOrDefault(comp => comp.ComponentId == c.ID);
                    if(comp != null)
                    {
                        comp.CannotSeeDescription = c.CannotSeeDescription;
                        comp.CannotSee = c.CannotSee;
                    }
                    else
                    {
                        MauiProgram._context.MissingViewRightsComponents.Add(new() { ComponentId = c.ID, UserId = _user.Id, CannotSee = c.CannotSee, CannotSeeDescription = c.CannotSeeDescription });
                    }
                }
                else
                {
                    if (components.Any(comp => comp.Id == c.ID))
                        MauiProgram._context.MissingViewRightsComponents.Remove(components.Single(co => co.Id == c.ID));
                }
            }
        });
        MauiProgram._context.SaveChanges();

        //recipes
        DynamicTableControlRecipe.ItemsSource.ToList().ForEach(item =>
        {
            var recipes = MauiProgram._context.MissingViewRightsRecipes.Where(r => r.UserId == _user.Id).ToList();
            if (item is MissingViewRightRecipeItem r) // Pattern Matching is neccessary
            {
                if (r.CannotSee || r.CannotSeeDescription || r.CannotSeeComponents)
                {
                    var rec = recipes.SingleOrDefault(rec => rec.RecipeId == r.ID);
                    if (rec != null)
                    {
                        rec.CannotSeeDescription = r.CannotSeeDescription;
                        rec.CannotSee = r.CannotSee;
                        rec.CannotSeeComponents = r.CannotSeeComponents;
                    }
                    else
                    {
                        MauiProgram._context.MissingViewRightsRecipes.Add(new() { RecipeId = r.ID, UserId = _user.Id, CannotSee = r.CannotSee, CannotSeeDescription = r.CannotSeeDescription, CannotSeeComponents = r.CannotSeeComponents });
                    }
                }
                else
                {
                    if (recipes.Any(rec => rec.Id == r.ID))
                        MauiProgram._context.MissingViewRightsRecipes.Remove(recipes.Single(rec => rec.Id == r.ID));
                }
            }
        });
        MauiProgram._context.SaveChanges();

        App.Current!.MainPage = new UserRightPage(_user);
    }

    private void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        //TODO: Sicherheitsabfrage
        //TODO: Remove rights in right table first
        //TODO: Remove groups
        MauiProgram._context.Remove(_user);
        MauiProgram._context.SaveChanges();
        App.Current!.MainPage = new UserOverviewPage();
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        App.Current!.MainPage = new UserOverviewPage();
    }

    private void OnPromoteButtonClicked(object sender, EventArgs e)
    {
        _user.IsAdmin = !_user.IsAdmin;
        MauiProgram._context.SaveChanges();
        App.Current!.MainPage = new UserOverviewPage();
    }
}