namespace RecipeCatalog;

public partial class UserOverviewPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserOverviewPage"/> class.
    /// </summary>
	public UserOverviewPage()
	{
		InitializeComponent();
		DisplayUsers();
	}

    /// <summary>
    /// Displays a list of users in a grid layout. Each user is represented by a frame that contains their username and associated campaign name, if available.
    /// The grid is organized into multiple rows and columns, depending on the number of users.
    /// </summary>
    public void DisplayUsers()
	{
        var users = MauiProgram._context.Users.ToList();

        int numberOfColumns = 2;
        for (int i = 0; i < numberOfColumns; i++)
        {
            UserView.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        }

        for (int i = 0; i < (users.Count + numberOfColumns - 1) / numberOfColumns; i++)
        {
            UserView.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        for (int i = 0; i < users.Count; i++)
        {
            var c = users[i];
            var frame = new Frame
            {
                BorderColor = Color.Parse("Gray"),
                CornerRadius = 5,
                Padding = 10,
                HasShadow = true,
                BackgroundColor = Color.FromArgb("#50D3D3D3"),
                Content = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    Children =
                    {
                        new Label
                        {
                            Text = c.Username,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 16
                        },
                        new Label
                        {
                            Text = c.CampaignId != null ? MauiProgram._context.Campaigns.Where(g => g.Id == c.CampaignId).Select(g => g.Name).Single() : string.Empty,
                            FontSize = 9,
                            TextColor = Color.Parse("DarkGray"),
                        }
                    }
                }
            };

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => OnFrameTapped(c.Id);
            frame.GestureRecognizers.Add(tapGestureRecognizer);

            UserView.Children.Add(frame);
            Grid.SetRow(frame, i / numberOfColumns);
            Grid.SetColumn(frame, i % numberOfColumns);
        }
    }

    /// <summary>
    /// Handles the tap event on a user frame, navigating to the User Rights page to view or modify the selected user's rights.
    /// </summary>
    /// <param name="id">The unique identifier of the user whose rights are to be viewed.</param>
    /// <exception cref="Exception">Throws if navigating to the UserRights page fails due to missing user data or other issues.</exception>
    private static void OnFrameTapped(Guid id)
    {
        try 
        { 
            App.Current!.MainPage = new UserRightPage(MauiProgram._context.Users.Where(u => u.Id == id).Single());
        }
        catch (Exception ex)
        {
            //TODO: DoLater
        }
    }

    /// <summary>
    /// Handles the back navigation event, returning the user to the previous Search and View page.
    /// </summary>
    /// <param name="sender">The source of the event (usually the back button).</param>
    /// <param name="e">The event data.</param>
    /// <exception cref="Exception">Throws if navigation back to the Search and View page fails.</exception>
    private void OnBack(object sender, EventArgs e)
    {
        //TODO: Missing searchdata
        App.Current!.MainPage = new SearchAndViewPage();
    }
}