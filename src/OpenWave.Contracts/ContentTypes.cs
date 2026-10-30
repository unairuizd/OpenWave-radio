namespace OpenWave.Contracts;

/// <summary>
/// The fixed content types. Each value is also the Radio Browser tag that identifies it.
/// </summary>
public static class ContentTypes
{
    public static IReadOnlyList<OptionDto> All { get; } =
    [
        new("music", "Música"),
        new("news", "Noticias"),
        new("talk", "Charlas"),
        new("sports", "Deportes"),
    ];
}
