using System.Text.Json;

namespace OpenWave.Api.Services;

/// <summary>
/// The curated music facts, read once from <c>Resources/Facts.&lt;language&gt;.json</c> for every supported language.
/// Every file must have the same ids, in the same order, so that a position means the same fact in every language.
/// </summary>
public sealed class FactsCatalog
{
    private readonly Dictionary<string, string[]> _textsByLanguage = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _defaultLanguage;

    /// <param name="contentRootPath">The API's content root, where the <c>Resources</c> folder lives.</param>
    /// <param name="languages">The supported language codes; the first one is the fallback.</param>
    public FactsCatalog(string contentRootPath, IReadOnlyList<string> languages)
    {
        if (languages.Count == 0)
        {
            throw new ArgumentException("At least one language is required.", nameof(languages));
        }

        _defaultLanguage = languages[0];
        int[]? expectedIds = null;

        foreach (var language in languages)
        {
            var path = Path.Combine(contentRootPath, "Resources", $"Facts.{language}.json");
            var facts = JsonSerializer.Deserialize<List<FactEntry>>(File.ReadAllText(path), JsonOptions)
                ?? throw new InvalidOperationException($"{path} does not contain a JSON array.");
            if (facts.Count == 0 || facts.Any(f => string.IsNullOrWhiteSpace(f.Text)))
            {
                throw new InvalidOperationException($"{path} must contain at least one fact and every fact needs a text.");
            }

            var ids = facts.Select(f => f.Id).ToArray();
            expectedIds ??= ids;
            if (!ids.SequenceEqual(expectedIds))
            {
                throw new InvalidOperationException(
                    $"{path} does not have the same fact ids, in the same order, as Facts.{languages[0]}.json.");
            }

            _textsByLanguage[language] = facts.Select(f => f.Text).ToArray();
        }

        Count = expectedIds!.Length;
    }

    /// <summary>How many facts there are (the same in every language).</summary>
    public int Count { get; }

    /// <summary>The fact at a position, in the language when it is supported, otherwise in the default language.</summary>
    public string GetText(int index, string language)
    {
        var texts = _textsByLanguage.TryGetValue(language, out var found) ? found : _textsByLanguage[_defaultLanguage];
        return texts[index];
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private sealed record FactEntry(int Id, string Text);
}
