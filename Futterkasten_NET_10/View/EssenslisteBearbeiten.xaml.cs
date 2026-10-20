using Futterkasten.ViewModel;
using Futterkasten.Model;

namespace Futterkasten;

public partial class EssenslisteBearbeiten : ContentPage
{
	public EssenslisteBearbeiten()
	{
		InitializeComponent();
		BindingContext = new EssenBearbeitenViewModel();
	}
}