using RecipeCatalog.Models;
using System.Collections.ObjectModel;

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
    }

    public void LoadData()
    {
        LabelNameEntry.Text = "Benutzer: " + _user.Username;
        NameEntry.Text = _user.Username;
        GuidEntry.Text = _user.Id.ToString();

        //var baseSettings = new ObservableCollection<ComponentView>();
        //baseSettings.Add(new() { Name = "Can see Component", IsSelected = _user.CanSeeComponents });
        //baseSettings.Add(new() { Name = "Can see Recipes", IsSelected = _user.CanSeeRecipes });
        //BaseCollectionView.ItemsSource = baseSettings;

        //--------------change due to group, recipe and component-------------------------
        //var userRights = MauiProgram._context.MissingViewRights.Where(m => m.UserId == _user.Id).ToList();
        //var groupSettings = new ObservableCollection<ComponentView>();
        //MauiProgram._context.Groups.ToList().ForEach(c =>
        //{
        //    groupSettings.Add(new() { Name = c.GroupName, Id = c.Id, IsSelected = !userRights.Any(ur => ur.GroupId == c.Id) });
        //});
        //GroupCollectionView.ItemsSource = groupSettings;
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
        //(BaseCollectionView.ItemsSource as ObservableCollection<ComponentView>)!.ToList().ForEach(c =>
        //{
        //    if(c.Name == "Can see Component")
        //        _user.CanSeeComponents = c.IsSelected;

        //    if(c.Name == "Can see Recipes")
        //        _user.CanSeeRecipes = c.IsSelected;
        //});
        MauiProgram._context.Update(_user);

        (GroupCollectionView.ItemsSource as ObservableCollection<ComponentView>)!.ToList().ForEach(c =>
        {
            //var entry = MauiProgram._context.MissingViewRights.Where(m => m.UserId == _user.Id && m.GroupId == c.Id).SingleOrDefault();
            //if(entry != null && c.IsSelected)
            //{
            //    MauiProgram._context.MissingViewRights.Remove(entry);
            //}
            //else if(entry == null && !c.IsSelected)
            //{
            //    entry = new()
            //    {
            //        UserId = _user.Id,
            //        GroupId = c.Id,
            //    };
            //    MauiProgram._context.MissingViewRights.Add(entry);
            //}
        });

        MauiProgram._context.SaveChanges();
        App.Current!.MainPage = new UserRightPage(_user);
    }

    private void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        //TODO: Sicherheitsabfrage
        MauiProgram._context.Remove(_user);
        MauiProgram._context.SaveChanges();
        App.Current!.MainPage = new UserOverviewPage();
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        App.Current!.MainPage = new UserOverviewPage();
    }
}