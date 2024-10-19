using CommunityToolkit.Maui.Views;
using RecipeCatalog.Resources.Language;
using System.Globalization;
using System.Resources;

namespace RecipeCatalog.Popups;

public partial class SettingsPopup : Popup
{
    private readonly Page page;
	public SettingsPopup(Page reloadpage)
	{
        page = reloadpage;
		InitializeComponent();
        LoadPickerData();
    }

    /// <summary>
    /// Loads the initial data into the picker controls for the settings popup.
    /// This includes data source configuration, current username, and available languages.
    /// </summary>
    private void LoadPickerData()
    {
        DataSource.Text = MauiProgram.Configuration.GetSection("Connection:DataSource").Value;
        UsernameInput.Text = MauiProgram.CurrentUser.Username;

        List<CultureInfo> cultures = new List<CultureInfo>
        {
            new CultureInfo("en-GB"),
            new CultureInfo("de-DE"),
        };
        LanguagePicker.ItemsSource = cultures;
        LanguagePicker.ItemDisplayBinding = new Binding("EnglishName");
        LanguagePicker.SelectedIndex = 0;
    }

    /// <summary>
    /// Handles the event when the "Send" button is clicked.
    /// Updates the configuration with the new data source and selected language,
    /// updates the current user's username in the database, and then closes the popup.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Event data.</param>
    private void OnSendButtonClicked(object sender, EventArgs e)
    {
        MauiProgram.Configuration["Connection:DataSource"] = DataSource.Text;
        SetLanguage(LanguagePicker.SelectedItem.ToString());

        MauiProgram.CurrentUser.Username = UsernameInput.Text;
        MauiProgram._context.Users.Update(MauiProgram.CurrentUser);
        MauiProgram._context.SaveChanges();
        MauiProgram.CurrentUser = MauiProgram._context.Users.Single(u => u.Id == MauiProgram.CurrentUser.Id);

        Close();
    }

    /// <summary>
    /// Handles the event when the "Cancel" button is clicked.
    /// Closes the settings popup without applying any changes.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Event data.</param>
    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Sets the application language based on the selected culture.
    /// Updates the current thread's culture settings and refreshes the resource manager.
    /// </summary>
    /// <param name="culture">The culture code representing the selected language.</param>
    private void SetLanguage(string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        ResourceManager rm = AppLanguage.ResourceManager;
        rm.ReleaseAllResources();
        MauiProgram.Configuration["DefaultLanguage"] = culture;
        App.Current!.MainPage = page; //error, double settings 
    }
}