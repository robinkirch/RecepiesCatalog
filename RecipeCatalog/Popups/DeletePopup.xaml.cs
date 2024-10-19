using CommunityToolkit.Maui.Views;

namespace RecipeCatalog.Popups;

public partial class DeletePopup : Popup
{
	public DeletePopup()
	{
		InitializeComponent();
	}

    private void OnOkButtonClicked(object sender, EventArgs e)
    {
        Close(true);
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(false);
    }
}