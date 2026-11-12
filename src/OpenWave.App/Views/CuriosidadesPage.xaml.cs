using OpenWave.App.ViewModels;

namespace OpenWave.App.Views;

public partial class CuriosidadesPage : ContentPage
{
    private readonly CuriosidadesViewModel _viewModel;

    public CuriosidadesPage(CuriosidadesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // First visit, a new day, or back from the login page: the service decides whether a request is needed.
        _ = _viewModel.OnAppearingAsync();
    }
}
