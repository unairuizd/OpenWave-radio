using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Services;
using OpenWave.App.Services.Localization;
using OpenWave.Contracts;

namespace OpenWave.App.ViewModels;

/// <summary>
/// Preferencias: the default filters of Inicio. Choices change only the pending values; <c>Guardar</c> stores them,
/// <c>Descartar</c> brings back the saved ones and <c>Restablecer</c> stores "Todos" for all four at once.
/// </summary>
public partial class PreferenciasViewModel : ObservableObject
{
    private readonly PreferencesService _preferences;
    private readonly CatalogApiClient _catalog;
    private readonly Localizer _localizer = Localizer.Instance;

    private IReadOnlyList<PreferenceOption> _countries = [];
    private IReadOnlyList<PreferenceOption> _languages = [];
    private IReadOnlyList<PreferenceOption> _genres = [];

    public PreferenciasViewModel(PreferencesService preferences, CatalogApiClient catalog)
    {
        _preferences = preferences;
        _catalog = catalog;
        var saved = preferences.Current;
        PendingCountry = Option(saved.Country, PreferenceLabels.Country);
        PendingLanguage = Option(saved.Language, PreferenceLabels.Language);
        PendingGenre = Option(saved.Genre, PreferenceLabels.Genre);
        PendingContentType = saved.ContentType;
    }

    // Pending choices. Null is "Todos".
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountryText), nameof(Summary), nameof(HasChanges))]
    public partial PreferenceOption? PendingCountry { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LanguageText), nameof(Summary), nameof(HasChanges))]
    public partial PreferenceOption? PendingLanguage { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GenreText), nameof(Summary), nameof(HasChanges))]
    public partial PreferenceOption? PendingGenre { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Summary), nameof(HasChanges))]
    public partial string? PendingContentType { get; private set; }

    /// <summary>"Todos" followed by the content types, the pending one marked.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<ChoiceItem> ContentTypes { get; private set; } = [];

    public string CountryText => PendingCountry?.Label ?? _localizer["Filter_All"];

    public string LanguageText => PendingLanguage?.Label ?? _localizer["Filter_All"];

    public string GenreText => PendingGenre?.Label ?? _localizer["Filter_All"];

    /// <summary>
    /// The pending filters in words, such as "Emisoras de Spain en Spanish · Jazz", or "Todas las emisoras".
    /// No station count.
    /// </summary>
    public string Summary
    {
        get
        {
            var contentType = PendingContentType is null ? null : ContentTypeLabel(PendingContentType);
            if (PendingCountry is null && PendingLanguage is null && PendingGenre is null && contentType is null)
            {
                return _localizer["Summary_All"];
            }

            var text = _localizer["Summary_Stations"];
            if (PendingCountry is not null)
            {
                text += " " + _localizer.Format("Summary_From", PendingCountry.Label);
            }

            if (PendingLanguage is not null)
            {
                text += " " + _localizer.Format("Summary_In", PendingLanguage.Label);
            }

            foreach (var part in new[] { PendingGenre?.Label, contentType })
            {
                if (part is not null)
                {
                    text += PreferenceLabels.Separator + part;
                }
            }

            return text;
        }
    }

    public bool HasChanges
    {
        get
        {
            var saved = _preferences.Current;
            return !Same(PendingCountry?.Value, saved.Country)
                || !Same(PendingLanguage?.Value, saved.Language)
                || !Same(PendingGenre?.Value, saved.Genre)
                || !Same(PendingContentType, saved.ContentType);
        }
    }

    [ObservableProperty]
    public partial bool IsLoading { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenFilterCommand))]
    public partial bool IsLoaded { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? ErrorMessage { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatus))]
    public partial string? StatusMessage { get; private set; }

    public bool HasError => ErrorMessage is not null;

    public bool HasStatus => StatusMessage is not null;

    /// <summary>
    /// Loads the option lists the first time the page appears. A failure never touches the saved values.
    /// </summary>
    public Task OnAppearingAsync() => IsLoaded || IsLoading ? Task.CompletedTask : LoadAsync();

    [RelayCommand]
    private Task RetryAsync() => LoadAsync();

    /// <summary>Opens the selection page of País, Idioma or Género; the parameter is the <see cref="FilterKind"/>.</summary>
    [RelayCommand(CanExecute = nameof(IsLoaded), AllowConcurrentExecutions = false)]
    private Task OpenFilterAsync(string kindName)
    {
        var kind = Enum.Parse<FilterKind>(kindName);
        var (options, pending) = kind switch
        {
            FilterKind.Country => (_countries, PendingCountry),
            FilterKind.Language => (_languages, PendingLanguage),
            _ => (_genres, PendingGenre),
        };

        // A pending value that is not in the list (a saved value the API no longer returns) is offered too.
        if (pending is not null && !options.Any(o => Same(o.Value, pending.Value)))
        {
            options = [pending, .. options];
        }

        var request = new FilterRequest(kind, options, pending?.Value, chosen => Choose(kind, chosen));
        return Shell.Current.GoToAsync("filtro", new ShellNavigationQueryParameters { [nameof(FilterRequest)] = request });
    }

    [RelayCommand]
    private void SelectContentType(ChoiceItem item)
    {
        PendingContentType = item.Value.Length == 0 ? null : item.Value;
        StatusMessage = null;
        UpdateContentTypeSelection();
    }

    [RelayCommand]
    private void Save()
    {
        var pending = new UserPreferences(PendingCountry?.Value, PendingLanguage?.Value, PendingGenre?.Value, PendingContentType);
        _preferences.Save(pending);
        RecentFilters.Add(FilterKind.Country, pending.Country);
        RecentFilters.Add(FilterKind.Language, pending.Language);
        RecentFilters.Add(FilterKind.Genre, pending.Genre);
        OnPropertyChanged(nameof(HasChanges));
        StatusMessage = _localizer["Preferencias_Saved"];
    }

    [RelayCommand]
    private void Discard()
    {
        var saved = _preferences.Current;
        PendingCountry = Find(_countries, saved.Country, PreferenceLabels.Country);
        PendingLanguage = Find(_languages, saved.Language, PreferenceLabels.Language);
        PendingGenre = Find(_genres, saved.Genre, PreferenceLabels.Genre);
        PendingContentType = saved.ContentType;
        StatusMessage = null;
        UpdateContentTypeSelection();
    }

    // Works even when the lists could not be loaded: it only needs to store "Todos".
    [RelayCommand]
    private void Reset()
    {
        _preferences.Reset();
        PendingCountry = null;
        PendingLanguage = null;
        PendingGenre = null;
        PendingContentType = null;
        UpdateContentTypeSelection();
        OnPropertyChanged(nameof(HasChanges));
        StatusMessage = _localizer["Preferencias_Saved"];
    }

    private void Choose(FilterKind kind, PreferenceOption? chosen)
    {
        // "Todos" arrives as an option with a null value.
        var value = chosen?.Value is null ? null : chosen;
        switch (kind)
        {
            case FilterKind.Country:
                PendingCountry = value;
                break;
            case FilterKind.Language:
                PendingLanguage = value;
                break;
            default:
                PendingGenre = value;
                break;
        }

        StatusMessage = null;
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        StatusMessage = null;

        try
        {
            var countries = _catalog.GetCountriesAsync();
            var languages = _catalog.GetLanguagesAsync();
            var genres = _catalog.GetGenresAsync();
            var contentTypes = _catalog.GetContentTypesAsync();
            await Task.WhenAll(countries, languages, genres, contentTypes);

            _countries = ToOptions(countries.Result);
            _languages = ToOptions(languages.Result);
            _genres = ToOptions(genres.Result);

            // The pending values take the labels of the lists (a saved value keeps its own label when it is missing).
            PendingCountry = Find(_countries, PendingCountry?.Value, PreferenceLabels.Country);
            PendingLanguage = Find(_languages, PendingLanguage?.Value, PreferenceLabels.Language);
            PendingGenre = Find(_genres, PendingGenre?.Value, PreferenceLabels.Genre);

            var contentTypeItems = new List<ChoiceItem> { new(string.Empty, () => _localizer["Filter_All"]) };
            contentTypeItems.AddRange(contentTypes.Result.Select(c => new ChoiceItem(c.Value, () => c.Label)));

            // A saved content type that the API no longer lists is kept as a chip.
            if (PendingContentType is { } saved && !contentTypeItems.Any(i => Same(i.Value, saved)))
            {
                contentTypeItems.Add(new ChoiceItem(saved, () => PreferenceLabels.ContentType(saved)));
            }

            ContentTypes = contentTypeItems;
            UpdateContentTypeSelection();
            OnPropertyChanged(nameof(Summary));
            IsLoaded = true;
        }
        catch (StationRequestException ex)
        {
            ErrorMessage = _localizer[ex.Error == StationRequestError.Unreachable ? "Error_Unreachable" : "Error_CatalogueUnavailable"];
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateContentTypeSelection()
    {
        foreach (var item in ContentTypes)
        {
            item.IsSelected = Same(item.Value.Length == 0 ? null : item.Value, PendingContentType);
        }
    }

    private string ContentTypeLabel(string value) =>
        ContentTypes.FirstOrDefault(c => Same(c.Value, value))?.Name ?? PreferenceLabels.ContentType(value);

    private static IReadOnlyList<PreferenceOption> ToOptions(IReadOnlyList<OptionDto> options) =>
        options.Select(o => new PreferenceOption(o.Value, o.Label, o.StationCount)).ToList();

    // The option of the list with that value; a value missing from the list keeps a label of its own.
    private static PreferenceOption? Find(IReadOnlyList<PreferenceOption> options, string? value, Func<string, string> labelFor) =>
        value is null ? null : options.FirstOrDefault(o => Same(o.Value, value)) ?? new PreferenceOption(value, labelFor(value));

    private static PreferenceOption? Option(string? value, Func<string, string> labelFor) =>
        value is null ? null : new PreferenceOption(value, labelFor(value));

    private static bool Same(string? a, string? b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
