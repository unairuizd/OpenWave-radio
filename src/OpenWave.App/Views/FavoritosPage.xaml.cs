using OpenWave.App.ViewModels;

namespace OpenWave.App.Views;

public partial class FavoritosPage : ContentPage
{
    private readonly FavoritosViewModel _viewModel;

    public FavoritosPage(FavoritosViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.OnAppearingAsync();
    }
}
