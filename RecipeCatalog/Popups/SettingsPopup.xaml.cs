using CommunityToolkit.Maui.Views;

namespace RecipeCatalog.Popups;

public partial class SettingsPopup : Popup
{
	public SettingsPopup()
	{
		InitializeComponent();
	}
    private void OnSendButtonClicked(object sender, EventArgs e)
    {
        Close();
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close();
    }

}