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

        private void OnSendButtonClicked(object sender, EventArgs e)
        {
            var newGroup = MauiProgram._context.Campaigns.Add(new Campaign { Name = NameEntry.Text });
            MauiProgram._context.SaveChanges();
            Close(MauiProgram._context.Campaigns.Single(g => g.Name == NameEntry.Text));
        }

        private void OnCancelButtonClicked(object sender, EventArgs e)
        {
            Close(null);
        }
    }
}