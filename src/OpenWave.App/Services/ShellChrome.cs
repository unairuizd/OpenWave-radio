namespace OpenWave.App.Services;

/// <summary>
/// Makes the parts that the platform draws itself follow the chosen appearance: the header title of the pages that
/// keep the platform header (through a TitleView) and the radio button circles (platform code in Platforms/).
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
