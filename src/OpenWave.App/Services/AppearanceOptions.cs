namespace OpenWave.App.Services;

/// <summary>
/// A bundled font. <see cref="Name"/> is the display name, the stored id and the registered font alias.
/// Static fonts also bundle a bold file, registered as <see cref="BoldAlias"/>; variable fonts contain
/// their own bold weight and use <see cref="Name"/> for bold text too.
/// </summary>
public sealed record FontOption(string Name, string FileName, string? BoldFileName = null)
{
    public string BoldAlias => BoldFileName is null ? Name : Name + " Bold";
}

/// <summary>
/// A text size preset: the factor applied to the base font sizes.
/// </summary>
public sealed record FontScaleOption(string DisplayName, double Scale);

/// <summary>
/// The fonts and text sizes offered in Personalización.
/// </summary>
public static class AppearanceOptions
{
    public const string DefaultFont = "Ubuntu";
    public const double DefaultScale = 1.0;

    // Base sizes at scale 1.0; the FontSize* resources are these values times the selected scale.
    public const double CaptionSize = 12;
    public const double BodySize = 16;
    public const double SubtitleSize = 18;
    public const double TitleSize = 24;

    public static IReadOnlyList<FontOption> Fonts { get; } =
    [
        new(DefaultFont, "Ubuntu-Regular.ttf", "Ubuntu-Bold.ttf"),
        new("Dosis", "Dosis-Variable.ttf"),
        new("Caladea", "Caladea-Regular.ttf"),
        new("Cormorant Garamond", "CormorantGaramond-Regular.ttf", "CormorantGaramond-Bold.ttf"),
        new("Recursive", "Recursive-Variable.ttf"),
        new("Pixelify Sans", "PixelifySans-Variable.ttf"),
    ];

    public static IReadOnlyList<FontScaleOption> Scales { get; } =
    [
        new("Pequeño", 0.85),
        new("Normal", DefaultScale),
        new("Grande", 1.2),
        new("Muy grande", 1.4),
        new("Enorme", 1.6),
    ];
}
