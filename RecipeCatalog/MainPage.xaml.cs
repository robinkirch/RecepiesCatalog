﻿using CommunityToolkit.Maui.Views;
using RecipeCatalog.Data;
using RecipeCatalog.Models;
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


            connectionstring = connectionstring == string.Empty ? MauiProgram.Configuration.GetSection("Connection:DataSource").Value ?? string.Empty : connectionstring;
            if(connectionstring == string.Empty)
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

        private void CheckUser(List<User> users)
        {
            var activityIndicator = this.FindByName<ActivityIndicator>("ActSpinner");
            activityIndicator.IsRunning = true;

            var userkey = MauiProgram.Configuration.GetSection("Connection:UserKey").Value;
            if (userkey == string.Empty || userkey == null)
            {
                UserBlock.IsVisible = true;
                StatusText.Text = AppLanguage.Main_EnterUsername;
            }
            else
            {
                //Test
                MauiProgram.CurrentUser = users.Where(u => u.Id == Guid.Parse(userkey!)).Single();

                activityIndicator.IsRunning = false;
                ContinueBtn.IsVisible = true;
                StatusText.IsVisible = false;
                WaitText.IsVisible = false;
            }
        }

        private void OnContinue(object sender, EventArgs e)
        {
            App.Current!.MainPage = new SearchAndViewPage();
        }

        private async void OnSettings(object sender, EventArgs e)
        {
            await this.ShowPopupAsync(new SettingsPopup(new MainPage()));
        }

        private void OnEntryCompleted(object sender, EventArgs e)
        {
            ConnectionStringBlock.IsVisible = false;
            MauiProgram.Configuration["Connection:DataSource"] = ConnectionStringInput.Text;
            CheckConnection(MauiProgram.Configuration.GetSection("Connection:DataSource").Value!);
        }

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

            MauiProgram.Configuration["Connection:UserKey"] = guid.ToString();
            CheckUser(MauiProgram._context.Users.ToList());
        }
    }
}
