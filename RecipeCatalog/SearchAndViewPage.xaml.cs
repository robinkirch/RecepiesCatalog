using CommunityToolkit.Maui.Views;
using RecipeCatalog.Models;
using RecipeCatalog.Popups;
using RecipeCatalog.Resources.Language;
using System.Globalization;
using System.Resources;

namespace RecipeCatalog;

public partial class SearchAndViewPage : ContentPage
{
	public SearchAndViewPage()
	{
		InitializeComponent();
        if (MauiProgram.configuration.GetSection("Connection:SecretKey").Value != "+KJDS??oO(D=)o8d-ü3=lkdsa3!3")
            AdminArea.IsVisible = false;
	}

    private async void OnAddGroup(object sender, EventArgs e)
    {
        var popup = new AddGroupPopup();
        var result = (Group?)await this.ShowPopupAsync(popup);
        //TODO
    }

    private void OnAddComponent(object sender, EventArgs e)
    {
        App.Current.MainPage = new SearchAndViewPage();
    }

    private void OnAddRecipe(object sender, EventArgs e)
    {
        App.Current.MainPage = new SearchAndViewPage();
    }

    
    
    //doubeled
    private void OnSettings(object sender, EventArgs e)
    {
        //TODO: Change to Popup
        SetLanguage("de");
    }

    private void SetLanguage(string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        ResourceManager rm = AppLanguage.ResourceManager;
        rm.ReleaseAllResources();
        MauiProgram.configuration["DefaultLanguage"] = culture;
        App.Current.MainPage = new MainPage();
    }
}