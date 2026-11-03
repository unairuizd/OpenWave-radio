namespace OpenWave.App.Views.Controls;

/// <summary>
/// Reads a value from an item by its property path (for example "Station.Name"), so the controls never need to know
/// the item type.
/// </summary>
internal sealed class ItemBinding : BindableObject
{
    private static readonly BindableProperty ValueProperty =
        BindableProperty.Create("Value", typeof(object), typeof(ItemBinding));

    public static object? Read(object? item, string? path)
    {
        if (item is null || string.IsNullOrEmpty(path))
        {
            return null;
        }

        var reader = new ItemBinding { BindingContext = item };
        reader.SetBinding(ValueProperty, new Binding(path));
        return reader.GetValue(ValueProperty);
    }
}
