namespace OpenWave.Contracts;

/// <summary>
/// One choice in a filter list: the value sent to the station search and the text shown to the user.
/// </summary>
public sealed record OptionDto(string Value, string Label, int? StationCount = null);
