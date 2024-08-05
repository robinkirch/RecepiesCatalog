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
        DataSource.Text = MauiProgram.configuration.GetSection("Connection:DataSource").Value;

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
        MauiProgram.configuration["Connection:DataSource"] = DataSource.Text;
        var a = LanguagePicker.SelectedItem.ToString();
        SetLanguage(LanguagePicker.SelectedItem.ToString());
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
        MauiProgram.configuration["DefaultLanguage"] = culture;
        App.Current.MainPage = page; //error, double settings 
    }
}