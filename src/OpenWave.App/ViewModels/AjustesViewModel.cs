using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OpenWave.App.ViewModels;

public partial class AjustesViewModel : ObservableObject
{
    // Pushed onto Ajustes, so the back button (or the gear) returns here.
    [RelayCommand]
    private Task OpenPreferenciasAsync() => Shell.Current.GoToAsync("preferencias");

    [RelayCommand]
    private Task OpenPersonalizacionAsync() => Shell.Current.GoToAsync("personalizacion");

    [RelayCommand]
    private Task OpenCuriosidadesAsync() => Shell.Current.GoToAsync("curiosidades");
}
