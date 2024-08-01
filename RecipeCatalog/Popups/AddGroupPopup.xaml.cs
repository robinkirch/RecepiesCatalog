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
        Close(new Group { GroupName = NameEntry.Text });
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}