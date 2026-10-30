using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Services;
using OpenWave.Contracts;

namespace OpenWave.App.ViewModels;

/// <summary>
/// One choice in a Preferencias picker. A null <see cref="Value"/> is "Todos" (no filter).
/// </summary>
public sealed record PreferenceOption(string? Value, string Label);

public partial class PreferenciasViewModel(PreferencesService preferences, CatalogApiClient catalog) : ObservableObject
{
    private const string AllLabel = "Todos";
    private const string SavedMessage = "Preferencias guardadas.";
    private const string UnreachableMessage = "No hay conexión con el servidor.";
    private const string CatalogueUnavailableMessage = "No se pudo obtener el catálogo de emisoras.";

    [ObservableProperty]
    public partial IReadOnlyList<PreferenceOption> Countries { get; private set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<PreferenceOption> Languages { get; private set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<PreferenceOption> Genres { get; private set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<PreferenceOption> ContentTypeOptions { get; private set; } = [];

    [ObservableProperty]
    public partial PreferenceOption? SelectedCountry { get; set; }

    [ObservableProperty]
    public partial PreferenceOption? SelectedLanguage { get; set; }

    [ObservableProperty]
    public partial PreferenceOption? SelectedGenre { get; set; }

    [ObservableProperty]
    public partial PreferenceOption? SelectedContentType { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
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

    [RelayCommand(CanExecute = nameof(IsLoaded))]
    private void Save()
    {
        preferences.Save(new UserPreferences(
            SelectedCountry?.Value,
            SelectedLanguage?.Value,
            SelectedGenre?.Value,
            SelectedContentType?.Value));
        StatusMessage = SavedMessage;
    }

    // Works even when the lists could not be loaded: it only needs to store "Todos".
    [RelayCommand]
    private void Reset()
    {
        preferences.Reset();
        if (IsLoaded)
        {
            SelectedCountry = Countries[0];
            SelectedLanguage = Languages[0];
            SelectedGenre = Genres[0];
            SelectedContentType = ContentTypeOptions[0];
        }

        StatusMessage = SavedMessage;
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        StatusMessage = null;

        try
        {
            var countries = catalog.GetCountriesAsync();
            var languages = catalog.GetLanguagesAsync();
            var genres = catalog.GetGenresAsync();
            var contentTypes = catalog.GetContentTypesAsync();
            await Task.WhenAll(countries, languages, genres, contentTypes);

            var saved = preferences.Current;
            (Countries, SelectedCountry) = BuildOptions(countries.Result, saved.Country, PreferenceLabels.Country);
            (Languages, SelectedLanguage) = BuildOptions(languages.Result, saved.Language, PreferenceLabels.Language);
            (Genres, SelectedGenre) = BuildOptions(genres.Result, saved.Genre, PreferenceLabels.Genre);
            (ContentTypeOptions, SelectedContentType) = BuildOptions(contentTypes.Result, saved.ContentType, PreferenceLabels.ContentType);
            IsLoaded = true;
        }
        catch (StationRequestException ex)
        {
            ErrorMessage = ex.Error == StationRequestError.Unreachable ? UnreachableMessage : CatalogueUnavailableMessage;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// "Todos" first, then the API options. A saved value missing from the list is kept as an option
    /// and selected, so it is never dropped silently.
    /// </summary>
    private static (IReadOnlyList<PreferenceOption> Options, PreferenceOption Selected) BuildOptions(
        IReadOnlyList<OptionDto> apiOptions,
        string? savedValue,
        Func<string, string> labelFor)
    {
        var options = new List<PreferenceOption> { new(null, AllLabel) };
        options.AddRange(apiOptions.Select(o => new PreferenceOption(o.Value, o.Label)));

        if (savedValue is null)
        {
            return (options, options[0]);
        }

        var selected = options.FirstOrDefault(o => string.Equals(o.Value, savedValue, StringComparison.OrdinalIgnoreCase));
        if (selected is null)
        {
            selected = new PreferenceOption(savedValue, labelFor(savedValue));
            options.Insert(1, selected);
        }

        return (options, selected);
    }
}
