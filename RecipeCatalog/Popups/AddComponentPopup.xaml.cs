using CommunityToolkit.Maui.Views;
using RecipeCatalog.Models;

namespace RecipeCatalog.Popups;

public partial class AddComponentPopup : Popup
{
    private byte[]? selectedImage;
	public AddComponentPopup(Component? component = null)
	{
		InitializeComponent();
        LoadPickerData();

        if(component != null)
        {
            selectedImage = component.Image;
            NameEntry.Text = component.Name;
            DescriptionEntry.Text = component.Description;
            AliasesEntry.Text = string.Join(',', component.Aliases);
            GroupPicker.SelectedItem = component.GroupNavigation;
        }
    }

    private void LoadPickerData()
    {
        GroupPicker.ItemsSource = MauiProgram._context.Groups.ToList();
        GroupPicker.ItemDisplayBinding = new Binding("GroupName");
    }

    private async void OnSelectImageClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Wählen Sie ein Bild",
            FileTypes = FilePickerFileType.Images
        });

        if (result != null)
        {
            var stream = await result.OpenReadAsync();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                selectedImage = memoryStream.ToArray();
            }
        }
    }

    private void OnSendButtonClicked(object sender, EventArgs e)
    {
        var newComponents = MauiProgram._context.Components.Add(new Component {
            Image = selectedImage,
            Name = NameEntry.Text,
            Description = DescriptionEntry.Text,
            Aliases = AliasesEntry.Text.Split(','),
            GroupId = (GroupPicker.SelectedIndex != -1) ? ((Group)GroupPicker.SelectedItem).Id : null,

        });
        MauiProgram._context.SaveChanges();
        Close(MauiProgram._context.Components.Single(c => c.Name == NameEntry.Text));
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}