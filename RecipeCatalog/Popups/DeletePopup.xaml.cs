using CommunityToolkit.Maui.Views;

namespace RecipeCatalog.Popups;

public partial class DeletePopup : Popup
{
	public DeletePopup()
	{
		InitializeComponent();
	}

    /// <summary>
    /// Handles the event when the "OK" button is clicked.
    /// Closes the popup and indicates a positive response (the user confirms the deletion).
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Event data.</param>
    private void OnOkButtonClicked(object sender, EventArgs e)
    {
        Close(true);
    }

    /// <summary>
    /// Handles the event when the "Cancel" button is clicked.
    /// Closes the popup and indicates a negative response (the user cancels the deletion).
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Event data.</param>
    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(false);
    }
}