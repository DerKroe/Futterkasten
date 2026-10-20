namespace Futterkasten;

public partial class GoogleLink : ContentPage
{
	public GoogleLink()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Launcher.OpenAsync("https://drive.google.com/drive/folders/1g9-uCJQlnAlImsZQJUHhMUuPVYa04u0r");
    }
}