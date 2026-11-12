using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenWave.App.Services.Localization;
using OpenWave.Contracts;

namespace OpenWave.App.Services;

/// <summary>
/// The daily music facts shared by the Inicio strip and the Curiosidades page: one request per login, repeated on
/// retry, on a new UTC day and when the language changes. Only logged-in users get the facts: while logged out
/// nothing is requested and everything is cleared.
/// </summary>
public sealed partial class FactsService : ObservableObject
{
    public const int Days = 15;

    private readonly FactsApiClient _api;
    private readonly SessionService _session;
    private Task? _loading;

    public FactsService(FactsApiClient api, SessionService session)
    {
        _api = api;
        _session = session;
        _session.PropertyChanged += OnSessionPropertyChanged;
        Localizer.Instance.LanguageChanged += (_, _) => Invalidate();
    }

    /// <summary>Whether the facts can be shown at all: a user is logged in.</summary>
    public bool IsAvailable => _session.IsLoggedIn;

    [ObservableProperty]
    public partial bool IsLoading { get; private set; }

    /// <summary>A message in the app language when the last load failed; null otherwise.</summary>
    [ObservableProperty]
    public partial string? ErrorMessage { get; private set; }

    /// <summary>The last successful answer, or null.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TodayFact), nameof(PreviousFacts), nameof(OnThisDay), nameof(HasOnThisDay))]
    public partial DailyFactsDto? Daily { get; private set; }

    public FactDto? TodayFact => Daily?.Facts.FirstOrDefault();

    public IReadOnlyList<FactDto> PreviousFacts => Daily?.Facts.Skip(1).ToList() ?? [];

    public IReadOnlyList<MusicEventDto> OnThisDay => Daily?.OnThisDay ?? [];

    public bool HasOnThisDay => OnThisDay.Count > 0;

    /// <summary>
    /// Loads the facts when they are missing or belong to another day; does nothing while logged out, and after
    /// a failure waits for <see cref="RetryAsync"/> (so the page shows the error at once instead of a new attempt).
    /// Concurrent callers share the same request.
    /// </summary>
    public Task LoadIfNeededAsync()
    {
        if (!IsAvailable || ErrorMessage is not null)
        {
            return Task.CompletedTask;
        }

        if (_loading is { IsCompleted: false })
        {
            return _loading;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (Daily is not null && Daily.Date == today)
        {
            return Task.CompletedTask;
        }

        _loading = LoadAsync();
        return _loading;
    }

    /// <summary>Loads again after a failure (the Reintentar button).</summary>
    public Task RetryAsync()
    {
        ErrorMessage = null;
        Daily = null;
        return LoadIfNeededAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var daily = await _api.GetDailyAsync(Days);
            if (IsAvailable)
            {
                Daily = daily;
            }
        }
        catch (StationRequestException ex)
        {
            Daily = null;
            ErrorMessage = ex.Error == StationRequestError.Unreachable
                ? Localizer.Instance["Error_Unreachable"]
                : Localizer.Instance["Error_Unexpected"];
        }
        catch (Exception)
        {
            Daily = null;
            ErrorMessage = Localizer.Instance["Error_Unexpected"];
        }
        finally
        {
            IsLoading = false;
        }
    }

    // The texts come in the request language: a language change means a new request the next time they are asked for.
    private void Invalidate()
    {
        Daily = null;
        ErrorMessage = null;
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(SessionService.IsLoggedIn) or nameof(SessionService.CurrentUser)))
        {
            return;
        }

        OnPropertyChanged(nameof(IsAvailable));
        if (IsAvailable)
        {
            _ = LoadIfNeededAsync();
        }
        else
        {
            Invalidate();
        }
    }
}
