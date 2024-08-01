using CommunityToolkit.Maui.Views;
using RecipeCatalog.Models;

namespace RecipeCatalog.Popups;

public partial class AddComponentPopup : Popup
{
	public AddComponentPopup()
	{
		InitializeComponent();
	}

    private void OnSendButtonClicked(object sender, EventArgs e)
    {
        Close(new Component { });
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}