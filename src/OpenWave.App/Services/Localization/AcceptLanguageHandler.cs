using System.Net.Http.Headers;

namespace OpenWave.App.Services.Localization;

/// <summary>
/// Adds the app's language (not the device's) to every request, so the API answers its own texts in it.
/// </summary>
public sealed class AcceptLanguageHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.AcceptLanguage.Clear();
        request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(Localizer.Instance.LanguageCode));
        return base.SendAsync(request, cancellationToken);
    }
}
