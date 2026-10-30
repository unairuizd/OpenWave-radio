using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Services;
using OpenWave.Contracts;

namespace OpenWave.App.ViewModels;

public partial class ReproductorViewModel : ObservableObject
{
    private readonly PlaybackService _playback;
    private readonly FavoritesService _favorites;

    public ReproductorViewModel(PlaybackService playback, FavoritesService favorites)
    {
        _playback = playback;
        _favorites = favorites;
        _playback.PropertyChanged += OnPlaybackPropertyChanged;
        _favorites.FavoritesChanged += (_, _) => OnPropertyChanged(nameof(FavoriteGlyph));
    }

    public string FavoriteGlyph =>
        _playback.CurrentStation is { } station && _favorites.IsFavorite(station.Id)
            ? FavoritesService.FilledHeart
            : FavoritesService.EmptyHeart;

    public StationDto? CurrentStation => _playback.CurrentStation;

    public string? StationName => _playback.CurrentStation?.Name;

    public string? Country => _playback.CurrentStation?.Country;

    public bool HasCountry => !string.IsNullOrEmpty(Country);

    public string? LogoUrl => _playback.CurrentStation?.LogoUrl;

    public string StatusText => _playback.StatusText;

    public string? ErrorMessage => _playback.ErrorMessage;

    public bool HasStation => _playback.CurrentStation is not null;

    public bool HasNoStation => _playback.CurrentStation is null;

    public bool HasError => HasStation && _playback.State == PlaybackState.Error;

    /// <summary>
    /// The play/stop control is shown for a selected station unless it is in the error state.
    /// </summary>
    public bool ShowPlayerControls => HasStation && !HasError;

    public string ToggleButtonText => _playback.IsActive ? "Detener" : "Reproducir";

    [RelayCommand]
    private Task ToggleAsync()
    {
        return _playback.ToggleAsync();
    }

    [RelayCommand]
    private Task RetryAsync()
    {
        return _playback.RetryAsync();
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task ToggleFavoriteAsync()
    {
        return _playback.CurrentStation is { } station
            ? _favorites.ToggleFromHeartAsync(station)
            : Task.CompletedTask;
    }

    private void OnPlaybackPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CurrentStation));
        OnPropertyChanged(nameof(StationName));
        OnPropertyChanged(nameof(Country));
        OnPropertyChanged(nameof(HasCountry));
        OnPropertyChanged(nameof(LogoUrl));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(ErrorMessage));
        OnPropertyChanged(nameof(HasStation));
        OnPropertyChanged(nameof(HasNoStation));
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(ShowPlayerControls));
        OnPropertyChanged(nameof(ToggleButtonText));
        OnPropertyChanged(nameof(FavoriteGlyph));
    }
}
