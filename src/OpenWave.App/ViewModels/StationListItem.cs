using CommunityToolkit.Mvvm.ComponentModel;
using OpenWave.Contracts;

namespace OpenWave.App.ViewModels;

/// <summary>
/// A station row in Inicio, with whether it is the station currently selected in the app.
/// </summary>
public partial class StationListItem(StationDto station) : ObservableObject
{
    public StationDto Station { get; } = station;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
