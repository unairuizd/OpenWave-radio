namespace OpenWave.App.Services;

/// <summary>
/// Makes the parts of the Shell that the platform draws itself follow the chosen font and text size:
/// the page header title (through a TitleView) and the tab labels (platform code in Platforms/).
/// </summary>
public static partial class ShellChrome
{
    /// <summary>
    /// Gives the page a header label styled like the rest of the app, unless it already has one.
    /// </summary>
    public static void EnsureTitleView(Page page)
    {
        if (Shell.GetTitleView(page) is not null || string.IsNullOrEmpty(page.Title))
        {
            return;
        }

        var title = new Label { Text = page.Title, VerticalOptions = LayoutOptions.Center };
        title.SetDynamicResource(VisualElement.StyleProperty, "HeaderTitleLabel");
        Shell.SetTitleView(page, title);
    }

    /// <summary>
    /// Applies the current font family and size to the tab labels.
    /// </summary>
    public static void ApplyTabFonts(Shell shell)
    {
        var resources = Application.Current?.Resources;
        if (resources is null
            || !resources.TryGetValue("AppFontFamily", out var family)
            || !resources.TryGetValue("FontSizeCaption", out var size))
        {
            return;
        }

        ApplyPlatformTabFonts(shell, (string)family, (double)size);
    }

    static partial void ApplyPlatformTabFonts(Shell shell, string fontFamily, double fontSize);

    /// <summary>
    /// Colours the native radio button circles from the theme. Android paints them once, when the activity
    /// starts, so after a live theme switch they would otherwise keep the old theme's colour.
    /// </summary>
    public static void ApplyRadioColors()
    {
        var resources = Application.Current?.Resources;
        if (resources is null
            || !resources.TryGetValue("AccentColor", out var accent)
            || !resources.TryGetValue("SecondaryTextColor", out var secondary))
        {
            return;
        }

        ApplyPlatformRadioColors((Color)accent, (Color)secondary);
    }

    static partial void ApplyPlatformRadioColors(Color checkedColor, Color uncheckedColor);
}
