using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecipeCatalog.Data;
using System.Globalization;
using CommunityToolkit.Maui;
using RecipeCatalog.Models;
using Microsoft.Maui.LifecycleEvents;

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
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            builder.Configuration.AddConfiguration(config);
            Configuration = builder.Configuration;

            var defLanguage = new CultureInfo(Configuration.GetSection("DefaultLanguage").Value ?? "en");
            CultureInfo.DefaultThreadCurrentCulture = defLanguage;
            CultureInfo.DefaultThreadCurrentUICulture = defLanguage;

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

        /// <summary>
        /// Determines whether the current user has admin privileges.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the user is an admin; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method checks the secret key stored in the configuration section 
        /// "Connection:SecretKey". If the value matches the expected admin key, the user is considered an admin.
        /// </remarks>
        public static bool IsThisUserAdmin() => Configuration.GetSection("Connection:SecretKey").Value == "+KJDS??oO(D=)o8d-ü3=lkdsa3!3";
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
    /// <br />- <b>Groups</b>: Represents filtering by groups or any value above (dynamically defined).
    /// <br />- <b>OffSet</b>: Used for calculating offsets in relation to group selections.
    /// </summary>
    public enum Selection
    {
        All = 0,
        Components = 1,
        Recipes = 2,
        Groups = 3,
        OffSet = Recipes,
    }
}
