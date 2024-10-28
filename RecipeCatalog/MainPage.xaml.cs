using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RecipeCatalog.Data;
using RecipeCatalog.Models;
using RecipeCatalog.Popups;
using RecipeCatalog.Resources.Language;
using System.Text.Json;

namespace RecipeCatalog
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            CheckAppData();
            CheckConnection();
        }

        private void CheckAppData()
        {
            var configDirectoryPath = RecipeCatalog.Manager.ConfigurationManager.GetDirectoryPath();
            if (!Directory.Exists(configDirectoryPath))
            {
                Directory.CreateDirectory(configDirectoryPath);
            }
            var configFilePath = RecipeCatalog.Manager.ConfigurationManager.GetFilePath();
            if (!File.Exists(configFilePath))
            {
                File.Create(configFilePath).Dispose();
                var configurationData = new Dictionary<string, string>();
                foreach (var kvp in MauiProgram.Configuration.AsEnumerable())
                {
                    configurationData[kvp.Key] = kvp.Value;
                }
                File.WriteAllText(configFilePath, System.Text.Json.JsonSerializer.Serialize(configurationData, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        /// <summary>
        /// Checks the connection to the database using the provided connection string.
        /// If no connection string is provided, it retrieves it from the configuration settings.
        /// Displays the appropriate status messages and UI elements based on the connection status.
        /// </summary>
        /// <param name="connectionstring">The connection string to use for the database connection.</param>
        private void CheckConnection(string connectionstring = "")
        {
            var activityIndicator = this.FindByName<ActivityIndicator>("ActSpinner");
            activityIndicator.IsRunning = true;
            StatusText.Text = AppLanguage.Main_CheckingConnection;


            connectionstring = connectionstring == string.Empty ? RecipeCatalog.Manager.ConfigurationManager.ReadValue("Connection:DataSource") : connectionstring;
            if(connectionstring == null || connectionstring == string.Empty)
            {
                ConnectionStringBlock.IsVisible = true;
                StatusText.Text = AppLanguage.Main_ConnectionInput;
            }
            else
            {
                //TODO: Test mit catch
                MauiProgram._context = new Context(connectionstring);
                CheckUser(MauiProgram._context.Users.ToList());
            }
        }

        /// <summary>
        /// Checks if a user exists based on the provided list of users and the user key from configuration.
        /// If the user key is not found or is empty, prompts for a username.
        /// If a valid user key is found, sets the current user and updates the UI accordingly.
        /// </summary>
        /// <param name="users">The list of users to check against.</param>
        private void CheckUser(List<User> users)
        {
            var activityIndicator = this.FindByName<ActivityIndicator>("ActSpinner");
            activityIndicator.IsRunning = true;

            var userkey = RecipeCatalog.Manager.ConfigurationManager.ReadValue("Connection:UserKey");
            if (userkey == string.Empty || userkey == null)
            {
                UserBlock.IsVisible = true;
                StatusText.Text = AppLanguage.Main_EnterUsername;
            }
            else
            {
                //Test
                MauiProgram.CurrentUser = users.Where(u => u.Id == Guid.Parse(userkey!)).Single();

                if (MauiProgram.CurrentUser != null && MauiProgram.CurrentUser.IsAdmin)
                    ShowStatistics();

                activityIndicator.IsRunning = false;
                ContinueBtn.IsVisible = true;
                StatusText.IsVisible = false;
                WaitText.IsVisible = false;
            }
        }

        /// <summary>
        /// Handles the event when the continue button is clicked.
        /// Navigates to the <see cref="SearchAndViewPage"/> page.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnContinue(object sender, EventArgs e)
        {
            App.Current!.MainPage = new SearchAndViewPage();
        }

        /// <summary>
        /// Handles the event when the settings button is clicked.
        /// Displays the settings popup for user configuration.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnSettings(object sender, EventArgs e)
        {
            await this.ShowPopupAsync(new SettingsPopup(new MainPage()));
        }

        /// <summary>
        /// Handles the event when the connection string entry is completed.
        /// Hides the connection string block and updates the configuration with the new connection string.
        /// Initiates a connection check with the updated connection string.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnEntryCompleted(object sender, EventArgs e)
        {
            ConnectionStringBlock.IsVisible = false;
            RecipeCatalog.Manager.ConfigurationManager.UpdateValue("Connection:DataSource", ConnectionStringInput.Text);
            CheckConnection(RecipeCatalog.Manager.ConfigurationManager.ReadValue("Connection:DataSource"));
        }

        /// <summary>
        /// Handles the event when the username entry is completed. Creates a new user with a unique identifier and adds it to the context.
        /// Updates the configuration with the new user key and starts the check from the beginning, which should be sending the user to the SearchAndViewPage
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnUserEntryCompleted(object sender, EventArgs e)
        {
            UserBlock.IsVisible = false;
            Guid? guid = null;
            do
            {
                guid = Guid.NewGuid();
            }
            while (MauiProgram._context.Users.Any(u => u.Id == guid.Value));

            User user = new()
            {
                Id = guid.Value,
                Username = UserStringInput.Text,
                IsAdmin = MauiProgram._context.Users.Any() ? false : true
            };
            MauiProgram._context.Users.Add(user);
            MauiProgram._context.SaveChanges();

            RecipeCatalog.Manager.ConfigurationManager.UpdateValue("Connection:UserKey", guid.ToString());
            CheckUser(MauiProgram._context.Users.ToList());
        }

        private void ShowStatistics()
        {
            StatisticsBlock.IsVisible = true;
            StatisticCountComponentsText.Text += MauiProgram._context.Components.Count();
            StatisticCountRecipesText.Text += MauiProgram._context.Recipes.Count();
        }
    }
}
