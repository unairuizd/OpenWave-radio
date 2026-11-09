using OpenWave.App.ViewModels;

namespace OpenWave.App.Views;

public partial class FiltroPage : ContentPage
{
    public FiltroPage(FiltroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
