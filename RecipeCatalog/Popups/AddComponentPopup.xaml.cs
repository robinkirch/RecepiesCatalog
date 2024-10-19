using CommunityToolkit.Maui.Views;
using RecipeCatalog.Models;
using System.Collections.ObjectModel;

namespace RecipeCatalog.Popups;

public partial class AddComponentPopup : Popup
{
    private readonly User _user;

    public AddComponentPopup(User user)
	{
        _user = user;
		InitializeComponent();
        LoadData();
        LoadPickerData();
    }

    private void LoadData()
    {
        var userRights = MauiProgram._context.MissingViewRightsComponents.Where(m => m.UserId == _user.Id).ToList();
        var viewSettings = new ObservableCollection<UserView>();
        var descSettings = new ObservableCollection<UserView>();
        MauiProgram._context.Users.ToList().ForEach(u =>
        {
            viewSettings.Add(new() { UserName = u.Username, Id = u.Id, IsSelected = false });
            descSettings.Add(new() { UserName = u.Username, Id = u.Id, IsSelected = false });
        });
        ViewCollectionView.ItemsSource = viewSettings;
        DescCollectionView.ItemsSource = descSettings;
    }

    private void LoadPickerData()
    {
        GroupPicker.ItemsSource = MauiProgram._context.Groups.ToList();
        GroupPicker.ItemDisplayBinding = new Binding("GroupName");
    }

    private void OnSendButtonClicked(object sender, EventArgs e)
    {
        var newComponents = MauiProgram._context.Components.Add(new Component {
            Image = null, // set in DetailPage
            Name = NameEntry.Text,
            Description = DescriptionEntry.Text,
            Aliases = (AliasesEntry.Text != null) ? AliasesEntry.Text.Split(',') : [],
            GroupId = (GroupPicker.SelectedIndex != -1) ? ((Group)GroupPicker.SelectedItem).Id : null,

        });
        MauiProgram._context.SaveChanges();

        List<UserView> descs = (DescCollectionView.ItemsSource as ObservableCollection<UserView>)!.ToList();
        (ViewCollectionView.ItemsSource as ObservableCollection<UserView>)!.ToList().ForEach(u =>
        {
            bool descSelected = descs.Where(d => d.Id == u.Id).Single().IsSelected;
            var entry = MauiProgram._context.MissingViewRightsComponents.Where(m => m.UserId == u.Id && m.ComponentId == newComponents.Entity.Id).SingleOrDefault();
            if (entry == null && (u.IsSelected || descSelected))
            {
                entry = new()
                {
                    UserId = u.Id,
                    ComponentId = newComponents.Entity.Id,
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
            MauiProgram._context.SaveChanges();
        });

        Close(MauiProgram._context.Components.Single(c => c.Name == NameEntry.Text));
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}