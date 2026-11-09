using System.Globalization;

namespace OpenWave.App.Services.Localization;

/// <summary>
/// Formats a translated text with values, in a MultiBinding: the first binding is the format, read from
/// <see cref="Localizer"/> (so the text follows language changes), the others are its arguments. For example:
/// <code>
/// &lt;MultiBinding Converter="{x:Static loc:FormatConverter.Instance}"&gt;
///     &lt;Binding Path="[Fmt_RemoveFavorite]" Source="{x:Static loc:Localizer.Instance}" x:DataType="loc:Localizer" /&gt;
///     &lt;Binding Path="Station.Name" /&gt;
/// &lt;/MultiBinding&gt;
/// </code>
/// </summary>
public sealed class FormatConverter : IMultiValueConverter
{
    public static FormatConverter Instance { get; } = new();

    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length == 0 || values[0] is not string format)
        {
            return string.Empty;
        }

        return string.Format(CultureInfo.CurrentCulture, format, values.Skip(1).ToArray());
    }

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
