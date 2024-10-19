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
        UsernameInput.Text = MauiProgram._context.Users.Where(u => u.Id == Guid.Parse(MauiProgram.Configuration.GetSection("Connection:UserKey").Value!)).Select(u => u.Username).Single();

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

        var user = MauiProgram._context.Users.Where(u => u.Id == Guid.Parse(MauiProgram.Configuration.GetSection("Connection:UserKey").Value!)).Single();
        user.Username = UsernameInput.Text;
        MauiProgram._context.Users.Update(user);
        MauiProgram._context.SaveChanges();

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