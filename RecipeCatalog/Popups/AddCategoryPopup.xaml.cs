using CommunityToolkit.Maui.Views;
using RecipeCatalog.Models;

namespace RecipeCatalog.Popups;

public partial class AddCategoryPopup : Popup
{
	public AddCategoryPopup()
	{
		InitializeComponent();
	}

    /// <summary>
    /// Handles the event when the "Send" button is clicked.
    /// Adds the new group to the database and closes the popup, ensuring that the newly created group is not null.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Event data.</param>
    /// <exception cref="NullReferenceException">Thrown when the new group could not be created.</exception>
    private void OnSendButtonClicked(object sender, EventArgs e)
    {
        var newCategory = MauiProgram._context.Categories.Add(new Category { CategoryName = NameEntry.Text }) ?? throw new NullReferenceException();
        MauiProgram._context.SaveChanges();
        Close(MauiProgram._context.Categories.Single(g => g.CategoryName == NameEntry.Text));
    }

    /// <summary>
    /// Handles the event when the "Cancel" button is clicked.
    /// Closes the popup without adding a new group or making changes.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Event data.</param>
    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}