using CommunityToolkit.Maui.Views;

namespace RecipeCatalog.Popups;

public partial class DeleteErrorPopup : Popup
{
	public DeleteErrorPopup(List<string> recipes)
	{
		InitializeComponent();
        AddUsedIn(recipes);

    }

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

    private void OnOkButtonClicked(object sender, EventArgs e)
    {
        Close(true);
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(false);
    }
}