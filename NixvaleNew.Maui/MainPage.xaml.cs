using NixvaleNew.Maui.ViewModels;

namespace NixvaleNew.Maui;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}

