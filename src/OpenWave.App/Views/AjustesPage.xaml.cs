using OpenWave.App.ViewModels;

namespace OpenWave.App.Views;

public partial class AjustesPage : ContentPage
{
    public AjustesPage(AjustesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
