using CommunityToolkit.Maui.Views;
using RecipeCatalog.Helper;
using RecipeCatalog.Models;
using RecipeCatalog.Resources.Language;
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

    /// <summary>
    /// Loads the data related to the user view and description view rights into the collection views.
    /// Initializes the data for the rights to view and edit component descriptions.
    /// </summary>
    private void LoadData()
    {
        var userItems = new ObservableCollection<MissingViewRightComponentUserItem>();
        MauiProgram._context.Users.ToList().ForEach(u =>
        {
            userItems.Add(new MissingViewRightComponentUserItem { UserID = u.Id, UserName = u.Username, CannotSee = false, CannotSeeDescription = false });
        });
        DynamicTableControlRights.ItemsSource = new ObservableCollection<object>(userItems.Cast<object>());
        DynamicTableControlRights.BuildTable(AppLanguage.User_CustomRights);
    }

    /// <summary>
    /// Loads the group data into the picker, allowing users to select the group for the component.
    /// </summary>
    private void LoadPickerData()
    {
        GroupPicker.ItemsSource = MauiProgram._context.Groups.ToList();
        GroupPicker.ItemDisplayBinding = new Binding("GroupName");
    }

    /// <summary>
    /// Handles the event when the "Send" button is clicked.
    /// Adds the new component to the database and sets the view rights for the selected users.
    /// Updates or adds the rights for users to view or not view the component and its description.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Event data.</param>
    private void OnSendButtonClicked(object sender, EventArgs e)
    {
        if (!MauiProgram._context.Components.Any(c => c.Name == NameEntry.Text))
        {
            var newComponents = MauiProgram._context.Components.Add(new Component
            {
                Image = null, // set in DetailPage
                Name = NameEntry.Text,
                Description = DescriptionEntry.Text,
                SecretDescription = SecretDescriptionEntry.Text,
                Aliases = (AliasesEntry.Text != null) ? AliasesEntry.Text.Split(',') : [],
                GroupId = (GroupPicker.SelectedIndex != -1) ? ((Group)GroupPicker.SelectedItem).Id : null,

            });
            MauiProgram._context.SaveChanges();

            DynamicTableControlRights.ItemsSource.ToList().ForEach(item =>
            {
                if (item is MissingViewRightComponentUserItem c && (c.CannotSee || c.CannotSeeDescription)) // Pattern Matching is neccessary
                        MauiProgram._context.MissingViewRightsComponents.Add(new() { ComponentId = newComponents.Entity.Id, UserId = c.UserID, CannotSee = c.CannotSee, CannotSeeDescription = c.CannotSeeDescription });
            });
            MauiProgram._context.SaveChanges();
        }
        //TODO: error display
        Close(MauiProgram._context.Components.Single(c => c.Name == NameEntry.Text));
    }

    /// <summary>
    /// Handles the event when the "Cancel" button is clicked.
    /// Closes the popup without adding a new component or making changes.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Event data.</param>
    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}