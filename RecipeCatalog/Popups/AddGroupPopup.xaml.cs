using CommunityToolkit.Maui.Views;
using RecipeCatalog.Models;

namespace RecipeCatalog.Popups;

public partial class AddGroupPopup : Popup
{
	public AddGroupPopup()
	{
		InitializeComponent();
	}

    private void OnSendButtonClicked(object sender, EventArgs e)
    {
        var newGroup = MauiProgram._context.Groups.Add(new Group { GroupName = NameEntry.Text });
        MauiProgram._context.SaveChanges();
        Close(MauiProgram._context.Groups.Single(g => g.GroupName == NameEntry.Text));
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}