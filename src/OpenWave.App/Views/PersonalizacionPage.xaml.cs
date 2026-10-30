using OpenWave.App.ViewModels;

namespace OpenWave.App.Views;

public partial class PersonalizacionPage : ContentPage
{
    public PersonalizacionPage(PersonalizacionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
