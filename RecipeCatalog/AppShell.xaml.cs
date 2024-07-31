namespace RecipeCatalog
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.Window.MinimumHeight = 300;
            this.Window.MinimumWidth = 300;
            this.Window.Width = 700;
            this.Window.Height = 1000;
        }
    }
}
