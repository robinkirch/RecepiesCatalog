using CommunityToolkit.Maui.Views;
using RecipeCatalog.Data;
using RecipeCatalog.Popups;
using RecipeCatalog.Resources.Language;

namespace RecipeCatalog
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            CheckConnection();
        }

        private void CheckConnection(string connectionstring = "")
        {
            var activityIndicator = this.FindByName<ActivityIndicator>("ActSpinner");
            activityIndicator.IsRunning = true;
            StatusText.Text = AppLanguage.Main_CheckingConnection;


            connectionstring = connectionstring == string.Empty ? MauiProgram.configuration.GetSection("Connection:DataSource").Value ?? string.Empty : connectionstring;
            if(connectionstring == string.Empty)
            {
                ConnectionStringBlock.IsVisible = true;
                StatusText.Text = AppLanguage.Main_ConnectionInput;
            }
            else
            {
                //TODO: Test
                MauiProgram._context = new Context(connectionstring);
                activityIndicator.IsRunning = false;
                ContinueBtn.IsVisible = true;
                StatusText.IsVisible = false;
                WaitText.IsVisible = false;
            }
        }

        private void OnContinue(object sender, EventArgs e)
        {
            App.Current.MainPage = new SearchAndViewPage();
        }

        private async void OnSettings(object sender, EventArgs e)
        {
            await this.ShowPopupAsync(new SettingsPopup(new MainPage()));
        }

        private void OnEntryCompleted(object sender, EventArgs e)
        {
            ConnectionStringBlock.IsVisible = false;
            MauiProgram.configuration["Connection:DataSource"] = ConnectionStringInput.Text;
            CheckConnection(MauiProgram.configuration.GetSection("Connection:DataSource").Value!);
        }
    }
}
