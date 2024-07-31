﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Maui.Platform;
using RecipeCatalog.Data;
using RecipeCatalog.Resources.Language;
using System;
using System.Globalization;
using System.Resources;

namespace RecipeCatalog
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            CheckConnection();
        }


        private void OnLanguageClicked(object sender, EventArgs e)
        {
            SetLanguage("de");
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
                var context = new Context(connectionstring);
                activityIndicator.IsRunning = false;
                //App.Current.MainPage = new MainPage();
                Shell.Current.GoToAsync($"{nameof(SearchAndViewPage)}");
            }
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

        private void OnEntryCompleted(object sender, EventArgs e)
        {
            ConnectionStringBlock.IsVisible = false;
            MauiProgram.configuration["Connection:DataSource"] = ConnectionStringInput.Text;
            CheckConnection(MauiProgram.configuration.GetSection("Connection:DataSource").Value!);
        }

    }
}
