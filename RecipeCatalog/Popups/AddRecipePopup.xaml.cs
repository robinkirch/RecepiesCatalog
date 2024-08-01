using CommunityToolkit.Maui.Views;
using RecipeCatalog.Models;

namespace RecipeCatalog.Popups;

public partial class AddRecipePopup : Popup
{
	public AddRecipePopup()
	{
		InitializeComponent();
	}
    private void OnSendButtonClicked(object sender, EventArgs e)
    {
        Close(new Recipe { });
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(null);
    }

}