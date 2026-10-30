using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OpenWave.App.ViewModels;

public partial class AjustesViewModel : ObservableObject
{
    // Pushed onto the Ajustes tab, so the back button returns here.
    [RelayCommand]
    private Task OpenPreferenciasAsync() => Shell.Current.GoToAsync("preferencias");

    [RelayCommand]
    private Task OpenPersonalizacionAsync() => Shell.Current.GoToAsync("personalizacion");
}
