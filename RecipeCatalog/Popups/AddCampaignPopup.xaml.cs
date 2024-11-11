using CommunityToolkit.Maui.Views;
using RecipeCatalog.Models;

namespace RecipeCatalog.Popups
{ 
    public partial class AddCampaignPopup : Popup
    {
        public AddCampaignPopup()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the event when the "Send" button is clicked.
        /// Adds a new campaign to the database using the name entered in the text field,
        /// saves the changes, and closes the popup with the newly added campaign.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">Event data.</param>
        private void OnSendButtonClicked(object sender, EventArgs e)
        {
            var newCategorie = MauiProgram._context.Campaigns.Add(new Campaign { Name = NameEntry.Text });
            MauiProgram._context.SaveChanges();
            Close(MauiProgram._context.Campaigns.Single(g => g.Name == NameEntry.Text));
        }

        /// <summary>
        /// Handles the event when the "Cancel" button is clicked.
        /// Closes the popup without performing any action.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">Event data.</param>
        private void OnCancelButtonClicked(object sender, EventArgs e)
        {
            Close(null);
        }
    }
}