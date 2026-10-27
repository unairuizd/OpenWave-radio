using OpenWave.App.ViewModels;

namespace OpenWave.App.Views;

public partial class FavoritosPage : ContentPage
{
    public FavoritosPage(FavoritosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
