using OpenWave.App.ViewModels;

namespace OpenWave.App.Views;

public partial class PreferenciasPage : ContentPage
{
    private readonly PreferenciasViewModel _viewModel;

    public PreferenciasPage(PreferenciasViewModel viewModel)
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
