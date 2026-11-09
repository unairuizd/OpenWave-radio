using OpenWave.App.Services;

namespace OpenWave.App.ViewModels;

/// <summary>
/// One option of a Preferencias filter. A null <see cref="Value"/> is "Todos" (no filter).
/// <see cref="StationCount"/> is the number of stations the API reported, when it did.
/// </summary>
public sealed record PreferenceOption(string? Value, string Label, int? StationCount = null);

/// <summary>
/// What Preferencias passes to a filter selection page: the filter, its options, the pending value and what to do
/// with the option the user chooses.
/// </summary>
public sealed record FilterRequest(
    FilterKind Kind,
    IReadOnlyList<PreferenceOption> Options,
    string? PendingValue,
    Action<PreferenceOption?> OnChosen);
