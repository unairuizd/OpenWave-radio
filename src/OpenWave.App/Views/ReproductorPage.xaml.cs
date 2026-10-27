using OpenWave.App.ViewModels;

namespace OpenWave.App.Views;

public partial class ReproductorPage : ContentPage
{
    public ReproductorPage(ReproductorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
