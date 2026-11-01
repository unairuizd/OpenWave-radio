using CommunityToolkit.Maui.Behaviors;

namespace OpenWave.App.Behaviors;

/// <summary>
/// Tints an icon with CommunityToolkit's <see cref="IconTintColorBehavior"/>. A behaviour is not part of the
/// visual tree, so it cannot read palette colours with DynamicResource; this attached property lives on the
/// image itself, where DynamicResource, styles and triggers work, and passes the colour on to the behaviour.
/// </summary>
public static class IconTint
{
    public static readonly BindableProperty ColorProperty = BindableProperty.CreateAttached(
        "Color", typeof(Color), typeof(IconTint), null, propertyChanged: OnColorChanged);

    public static Color? GetColor(BindableObject view) => (Color?)view.GetValue(ColorProperty);

    public static void SetColor(BindableObject view, Color? value) => view.SetValue(ColorProperty, value);

    // The behaviour does not always repaint when its colour changes after it is attached (Windows), so each change
    // gets a new behaviour.
    private static void OnColorChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not View view)
        {
            return;
        }

        foreach (var old in view.Behaviors.OfType<IconTintColorBehavior>().ToList())
        {
            view.Behaviors.Remove(old);
        }

        if (newValue is Color color)
        {
            view.Behaviors.Add(new IconTintColorBehavior { TintColor = color });
        }
    }
}
