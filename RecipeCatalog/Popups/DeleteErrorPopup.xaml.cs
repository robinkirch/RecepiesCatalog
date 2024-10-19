using CommunityToolkit.Maui.Views;

namespace RecipeCatalog.Popups;

public partial class DeleteErrorPopup : Popup
{
	public DeleteErrorPopup(List<string> recipes)
	{
		InitializeComponent();
        AddUsedIn(recipes);

    }

    /// <summary>
    /// Populates the popup with the recipes that are affected by the deletion error.
    /// Each recipe name is displayed in a separate row within the grid.
    /// </summary>
    /// <param name="recipes">A list of recipe names to be displayed in the popup.</param>
    public void AddUsedIn(List<string> recipes)
    {
        int row = 0;

        recipes.ForEach(r =>
        {
            RecipesComponentsPlace.RowDefinitions.Add(new RowDefinition { Height = 20 });
            //TODO: Link to detailpage
            var nameLabel = new Label
            {
                Text = r,
                VerticalOptions = LayoutOptions.Start
            };
            RecipesComponentsPlace.Children.Add(nameLabel);
            Grid.SetRow(nameLabel, row);
            Grid.SetColumn(nameLabel, 0);
            row++;
        });
    }

    /// <summary>
    /// Handles the event when the "OK" button is clicked.
    /// Closes the popup and indicates a positive response (user acknowledged the error).
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Event data.</param>
    private void OnOkButtonClicked(object sender, EventArgs e)
    {
        Close(true);
    }

    /// <summary>
    /// Handles the event when the "Cancel" button is clicked.
    /// Closes the popup and indicates a negative response (user chose not to proceed).
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Event data.</param>
    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(false);
    }
}