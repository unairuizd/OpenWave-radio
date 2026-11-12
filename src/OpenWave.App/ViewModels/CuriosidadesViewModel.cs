using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Services;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.ViewModels;

/// <summary>A fact with its date already written in the app language.</summary>
public sealed record FactItem(string DateText, string Text);

/// <summary>A "tal día como hoy" item: the year, the text and the Wikipedia article.</summary>
public sealed record MusicEventItem(string Year, string Text, string ArticleUrl)
{
    public bool HasArticle => !string.IsNullOrEmpty(ArticleUrl);
}

/// <summary>
/// The Curiosidades page: today's fact, what happened today in music, and the previous days. Everything comes from
/// the shared <see cref="FactsService"/>; logged out, the page only invites to log in.
/// </summary>
public partial class CuriosidadesViewModel : ObservableObject
{
    private readonly FactsService _facts;

    public CuriosidadesViewModel(FactsService facts)
    {
        _facts = facts;
        _facts.PropertyChanged += OnFactsChanged;
        Localizer.Instance.LanguageChanged += (_, _) => Refresh();
        Refresh();
    }

    public bool IsAvailable => _facts.IsAvailable;

    public bool IsLoggedOut => !_facts.IsAvailable;

    public bool IsLoading => _facts.IsLoading;

    public string? ErrorMessage => _facts.ErrorMessage;

    public bool HasError => _facts.ErrorMessage is not null;

    /// <summary>The three blocks are shown only with a loaded answer.</summary>
    public bool ShowContent => IsAvailable && _facts.Daily is not null;

    [ObservableProperty]
    public partial FactItem? Today { get; private set; }

    [ObservableProperty]
    public partial IReadOnlyList<MusicEventItem> OnThisDay { get; private set; } = [];

    public bool HasOnThisDay => OnThisDay.Count > 0;

    public bool ShowOnThisDayEmpty => ShowContent && !HasOnThisDay;

    [ObservableProperty]
    public partial IReadOnlyList<FactItem> PreviousFacts { get; private set; } = [];

    /// <summary>Called by the page when it is shown: loads when needed (first visit, a new day, after a login).</summary>
    public Task OnAppearingAsync() => _facts.LoadIfNeededAsync();

    [RelayCommand]
    private Task RetryAsync() => _facts.RetryAsync();

    [RelayCommand]
    private Task LogInAsync() => Shell.Current.GoToAsync("login");

    [RelayCommand]
    private async Task OpenArticleAsync(string? url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            await Launcher.Default.OpenAsync(uri);
        }
    }

    private void OnFactsChanged(object? sender, PropertyChangedEventArgs e) => Refresh();

    private void Refresh()
    {
        var format = Localizer.Instance["Facts_DateFormat"];
        var culture = CultureInfo.CurrentCulture;
        Today = _facts.TodayFact is { } fact ? new FactItem(fact.Date.ToString(format, culture), fact.Text) : null;
        PreviousFacts = _facts.PreviousFacts.Select(f => new FactItem(f.Date.ToString(format, culture), f.Text)).ToList();
        OnThisDay = _facts.OnThisDay
            .Select(e => new MusicEventItem(e.Year.ToString(culture), e.Text, e.ArticleUrl))
            .ToList();

        foreach (var name in new[]
                 {
                     nameof(IsAvailable), nameof(IsLoggedOut), nameof(IsLoading), nameof(ErrorMessage), nameof(HasError),
                     nameof(ShowContent), nameof(HasOnThisDay), nameof(ShowOnThisDayEmpty),
                 })
        {
            OnPropertyChanged(name);
        }
    }
}
