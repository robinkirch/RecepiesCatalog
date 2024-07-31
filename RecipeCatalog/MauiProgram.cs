using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecipeCatalog.Data;
using System;
using System.Globalization;
using Microsoft.Maui.LifecycleEvents;


namespace RecipeCatalog
{
    public static class MauiProgram
    {
        public static IConfiguration configuration {get; private set;}

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            builder.Configuration.AddConfiguration(config);
            configuration = builder.Configuration;

            var defLanguage = new CultureInfo(configuration.GetSection("DefaultLanguage").Value ?? "en");
            CultureInfo.DefaultThreadCurrentCulture = defLanguage;
            CultureInfo.DefaultThreadCurrentUICulture = defLanguage;

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif


#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>
        {
            // Make sure to add "using Microsoft.Maui.LifecycleEvents;" in the top of the file
            events.AddWindows(windowsLifecycleBuilder =>
            {
                windowsLifecycleBuilder.OnWindowCreated(window =>
                {
                    window.ExtendsContentIntoTitleBar = false;
                    var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
                    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);

                   if (appWindow is not null)
                    {
                        Microsoft.UI.Windowing.DisplayArea displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(id, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);
                        if (displayArea is not null)
                        {
                            var CenteredPosition = appWindow.Position;
                            CenteredPosition.X = ((displayArea.WorkArea.Width - appWindow.Size.Width) / 2);
                            CenteredPosition.Y = ((displayArea.WorkArea.Height - appWindow.Size.Height) / 2);
                            appWindow.Move(CenteredPosition);
                        }
                    }
                });
            });
        });
#endif

            return builder.Build();
        }
    }
}
