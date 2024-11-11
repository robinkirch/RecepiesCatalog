using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecipeCatalog.Data;
using System.Globalization;
using CommunityToolkit.Maui;
using RecipeCatalog.Models;
using Microsoft.Maui.LifecycleEvents;
using System;
using NLog.Extensions.Logging;
using NLog;
using RecipeCatalog.Resources.Language;

namespace RecipeCatalog
{
    public static class MauiProgram
    {
        public static IConfiguration Configuration {get; private set;}
        public static Context _context {get; set;}

        public static User CurrentUser;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            string filePath = string.Empty;
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                filePath = Path.Combine(FileSystem.AppDataDirectory, "appsettings.json");
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                filePath = "appsettings.json";
            }
            else
            {
                throw new PlatformNotSupportedException(AppLanguage.Exception_PlatformNotSupported);
            }
            var config = new ConfigurationBuilder()
                .AddJsonFile(filePath, optional: true, reloadOnChange: true)
                .Build();

            builder.Configuration.AddConfiguration(config);
            Configuration = builder.Configuration;
            var defLanguage = new CultureInfo(RecipeCatalog.Manager.ConfigurationManager.ReadValue("DefaultLanguage") ?? "en");
            CultureInfo.DefaultThreadCurrentCulture = defLanguage;
            CultureInfo.DefaultThreadCurrentUICulture = defLanguage;

            builder.Logging.ClearProviders();
            builder.Logging.AddNLog();
            NLog.LogManager.Setup().RegisterMauiLog().LoadConfigurationFromAssemblyResource(typeof(App).Assembly);
            var logger = LogManager.Setup().LoadConfigurationFromFile("NLog.config").GetCurrentClassLogger();
            logger.Info("NLog MAUI init.");

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Font Awesome 6 Brands-Regular-400.otf", "FontAwesomeBrands");
                    fonts.AddFont("Font Awesome 6 Free-Regular-400.otf", "FontAwesomeRegular");
                    fonts.AddFont("Font Awesome 6 Free-Solid-900.otf", "FontAwesomeSolid");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif


#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>
        {
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

        /// <summary>
        /// Converts a byte array representing image data into an <see cref="ImageSource"/>.
        /// </summary>
        /// <param name="imageData">An optional byte array containing the image data. 
        /// If <c>null</c> or empty, a default image will be returned.</param>
        /// <returns>
        /// An <see cref="ImageSource"/> representing the image. If the <paramref name="imageData"/> 
        /// is <c>null</c> or has a length of zero, a default image is returned; otherwise, 
        /// the image is created from the provided byte array.
        /// </returns>
        public static ImageSource ByteArrayToImageSource(byte[]? imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return ImageSource.FromFile("Resources/Images/no_image_by_riskywas.png");

            var stream = new MemoryStream(imageData);
            stream.Seek(0, SeekOrigin.Begin);
            return ImageSource.FromStream(() => stream);
        }
    }

    /// <summary>
    /// Enum representing selection options for filtering.
    /// <para>'Groups' is always the highest value and represents groups or higher, 
    /// since the actual number of group values is dynamic and stored in the database.</para>
    /// <para>The 'OffSet' value corresponds to 'Recipes' and is used for calculating 
    /// the offset when determining the selected index for groups.</para>
    /// <br />
    /// Enum values:
    /// <br />- <b>All</b>: Represents all options.
    /// <br />- <b>Components</b>: Represents filtering by components.
    /// <br />- <b>Recipes</b>: Represents filtering by recipes.
    /// <br />- <b>Categories</b>: Represents filtering by categories or any value above (dynamically defined).
    /// <br />- <b>OffSet</b>: Used for calculating offsets in relation to group selections.
    /// </summary>
    public enum Selection
    {
        All = 0,
        Components = 1,
        Recipes = 2,
        Categories = 3,
        OffSet = Recipes,
    }
}
