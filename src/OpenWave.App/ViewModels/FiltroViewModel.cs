using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Services;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.ViewModels;

/// <summary>
/// A line of a filter selection page: a section title, or an option with its station count and a tick when it is
/// the pending value.
/// </summary>
public sealed class FilterRow
{
    public bool IsHeader { get; init; }

    public bool IsOption => !IsHeader;

    public string Text { get; init; } = string.Empty;

    /// <summary>Under the option's label ("sin filtro" for Todos), or empty.</summary>
    public string Detail { get; init; } = string.Empty;

    public bool HasDetail => Detail.Length > 0;

    /// <summary>The number of stations in the app's number format, or empty.</summary>
    public string Count { get; init; } = string.Empty;

    public bool IsSelected { get; init; }

    public PreferenceOption? Option { get; init; }

    public string AccessibleName { get; init; } = string.Empty;
}

/// <summary>
/// The selection page of País, Idioma or Género: a search box, "Todos", the recent values and the full list.
/// Choosing an option hands it back to Preferencias and returns there.
/// </summary>
public partial class FiltroViewModel : ObservableObject, IQueryAttributable
{
    private readonly Localizer _localizer = Localizer.Instance;
    private FilterRequest? _request;

    [ObservableProperty]
    public partial string Title { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string SearchPlaceholder { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string Query { get; set; } = string.Empty;

    // An empty list shows "Sin resultados" (the CollectionView's EmptyView).
    [ObservableProperty]
    public partial IReadOnlyList<FilterRow> Rows { get; private set; } = [];

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue(nameof(FilterRequest), out var value) || value is not FilterRequest request)
        {
            return;
        }

        _request = request;
        var name = request.Kind.ToString();
        Title = _localizer[$"Filter_{name}"];
        SearchPlaceholder = _localizer[$"Filter_Search{name}"];
        Query = string.Empty;
        BuildRows();
    }

    partial void OnQueryChanged(string value) => BuildRows();

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task ChooseAsync(FilterRow row)
    {
        if (_request is null || row.IsHeader)
        {
            return;
        }

        _request.OnChosen(row.Option);
        await Shell.Current.GoToAsync("..");
    }

    private void BuildRows()
    {
        if (_request is null)
        {
            Rows = [];
            return;
        }

        var rows = new List<FilterRow>();
        var search = Normalise(Query);

        if (search.Length == 0)
        {
            var all = _localizer["Filter_All"];
            rows.Add(new FilterRow
            {
                Text = all,
                Detail = _localizer["Filter_NoFilter"],
                IsSelected = _request.PendingValue is null,
                Option = new PreferenceOption(null, all),
                AccessibleName = Accessible(all, _request.PendingValue is null),
            });

            var recent = RecentFilters.Get(_request.Kind)
                .Select(v => _request.Options.FirstOrDefault(o => Same(o.Value, v)) ?? new PreferenceOption(v, Label(v)))
                .ToList();
            if (recent.Count > 0)
            {
                rows.Add(new FilterRow { IsHeader = true, Text = _localizer["Filter_Recent"] });
                rows.AddRange(recent.Select(o => OptionRow(o, withCount: false)));
            }

            rows.Add(new FilterRow { IsHeader = true, Text = _localizer[$"Filter_AllOf{_request.Kind}"] });
            rows.AddRange(_request.Options.Select(o => OptionRow(o, withCount: true)));
        }
        else
        {
            rows.AddRange(_request.Options
                .Where(o => Normalise(o.Label).Contains(search, StringComparison.Ordinal))
                .Select(o => OptionRow(o, withCount: true)));
        }

        Rows = rows;
    }

    private FilterRow OptionRow(PreferenceOption option, bool withCount)
    {
        var selected = Same(option.Value, _request?.PendingValue);
        var count = withCount && option.StationCount is { } n ? n.ToString("N0", CultureInfo.CurrentCulture) : string.Empty;
        return new FilterRow
        {
            Text = option.Label,
            Count = count,
            IsSelected = selected,
            Option = option,
            AccessibleName = Accessible(count.Length > 0 ? $"{option.Label}, {count}" : option.Label, selected),
        };
    }

    private string Accessible(string name, bool selected) => selected ? _localizer.Format("Nav_Selected", name) : name;

    private string Label(string value) => _request?.Kind switch
    {
        FilterKind.Country => PreferenceLabels.Country(value),
        FilterKind.Language => PreferenceLabels.Language(value),
        _ => PreferenceLabels.Genre(value),
    };

    /// <summary>Lower case without accents, so "belgica" finds "Bélgica".</summary>
    private static string Normalise(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var c in text.Trim().Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(c));
            }
        }

        return builder.ToString();
    }

    private static bool Same(string? a, string? b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
