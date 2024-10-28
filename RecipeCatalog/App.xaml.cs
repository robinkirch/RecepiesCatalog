namespace RecipeCatalog
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (DeviceInfo.Platform == DevicePlatform.Android)
                _ = RequestPermissionsAsync();
        }

        public async Task RequestPermissionsAsync()
        {
            var readPermissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            var writePermissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (readPermissionStatus != PermissionStatus.Granted)
            {
                readPermissionStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
            }

            if (writePermissionStatus != PermissionStatus.Granted)
            {
                writePermissionStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();
            }

            if (readPermissionStatus != PermissionStatus.Granted || writePermissionStatus != PermissionStatus.Granted)
            {
                // TODO: Alert
            }
        }
    }
}