using CommunityToolkit.Maui.Views;
using RecipeCatalog.Resources.Language;
using System.Globalization;
using System.Resources;

namespace RecipeCatalog.Popups;

public partial class SettingsPopup : Popup
{
    private Page page;
	public SettingsPopup(Page reloadpage)
	{
        page = reloadpage;
		InitializeComponent();
        LoadPickerData();
    }

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

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close();
    }
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