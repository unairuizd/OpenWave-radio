using CommunityToolkit.Mvvm.ComponentModel;
using OpenWave.App.Services;
using OpenWave.Contracts;

namespace OpenWave.App.ViewModels;

/// <summary>
/// A station row in Inicio, with whether it is the station currently selected in the app
/// and whether it is one of the logged-in user's favourites.
/// </summary>
public partial class StationListItem(StationDto station) : ObservableObject
{
    public StationDto Station { get; } = station;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FavoriteGlyph))]
    public partial bool IsFavorite { get; set; }

    public string FavoriteGlyph => IsFavorite ? FavoritesService.FilledHeart : FavoritesService.EmptyHeart;
}
