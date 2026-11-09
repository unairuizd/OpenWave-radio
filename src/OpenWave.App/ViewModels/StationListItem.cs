using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenWave.Contracts;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.ViewModels;

/// <summary>
/// A station card in Inicio or Favoritos, with whether it is the station currently selected in the app
/// and whether it is one of the logged-in user's favourites. The owning view model sets the commands, so one
/// card template serves both lists; each command takes the item as its parameter.
/// </summary>
public partial class StationListItem(StationDto station) : ObservableObject
{
    private const int MaxTags = 3;
    private static string Unknown => Localizer.Instance["Station_Unknown"];

    public StationDto Station { get; } = station;

    /// <summary>
    /// For example "Spain · Spanish · pop · rock · news": country and language, then up to three tags.
    /// </summary>
    public string Details { get; } = string.Join(" · ",
        new[] { station.Country ?? Unknown, station.Language ?? Unknown }
            .Concat((station.Tags ?? []).Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).Take(MaxTags)));

    /// <summary>
    /// Accessible name of the card's play button.
    /// </summary>
    public string PlayDescription => Localizer.Instance.Format("Fmt_Play", Station.Name);

    /// <summary>
    /// Selects and plays the station (the card and its play button).
    /// </summary>
    public ICommand? PlayCommand { get; init; }

    /// <summary>
    /// The card's heart.
    /// </summary>
    public ICommand? HeartCommand { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }
}
