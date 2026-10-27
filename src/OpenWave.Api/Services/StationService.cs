using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// In-memory station catalogue used until the Radio Browser integration arrives.
/// </summary>
public sealed class StationService : IStationService
{
    private static readonly IReadOnlyList<StationDto> Stations =
    [
        new()
        {
            Id = "cadena-ser",
            Name = "Cadena SER",
            StreamUrl = "https://example.com/streams/cadena-ser.mp3",
            Country = "España",
            Language = "Español",
            Tags = ["noticias", "actualidad"],
            LogoUrl = "https://example.com/logos/cadena-ser.png"
        },
        new()
        {
            Id = "los40",
            Name = "LOS40",
            StreamUrl = "https://example.com/streams/los40.mp3",
            Country = "España",
            Language = "Español",
            Tags = ["pop", "éxitos"],
            LogoUrl = "https://example.com/logos/los40.png"
        },
        new()
        {
            Id = "radio-nacional",
            Name = "Radio Nacional de España",
            StreamUrl = "https://example.com/streams/radio-nacional.mp3",
            Country = "España",
            Language = "Español",
            Tags = ["cultura", "noticias"],
            LogoUrl = null
        },
        new()
        {
            Id = "kiss-fm",
            Name = "Kiss FM",
            StreamUrl = "https://example.com/streams/kiss-fm.mp3",
            Country = "España",
            Language = "Español",
            Tags = ["dance", "electrónica"],
            LogoUrl = "https://example.com/logos/kiss-fm.png"
        },
        new()
        {
            Id = "bbc-radio-1",
            Name = "BBC Radio 1",
            StreamUrl = "https://example.com/streams/bbc-radio-1.mp3",
            Country = "Reino Unido",
            Language = "Inglés",
            Tags = ["pop", "novedades"],
            LogoUrl = "https://example.com/logos/bbc-radio-1.png"
        },
        new()
        {
            Id = "bbc-radio-4",
            Name = "BBC Radio 4",
            StreamUrl = "https://example.com/streams/bbc-radio-4.mp3",
            Country = "Reino Unido",
            Language = "Inglés",
            Tags = ["noticias", "debate"],
            LogoUrl = null
        },
        new()
        {
            Id = "france-inter",
            Name = "France Inter",
            StreamUrl = "https://example.com/streams/france-inter.mp3",
            Country = "Francia",
            Language = "Francés",
            Tags = ["actualidad", "cultura"],
            LogoUrl = "https://example.com/logos/france-inter.png"
        },
        new()
        {
            Id = "rtl",
            Name = "RTL",
            StreamUrl = "https://example.com/streams/rtl.mp3",
            Country = "Francia",
            Language = "Francés",
            Tags = ["variedades", "entretenimiento"],
            LogoUrl = null
        },
        new()
        {
            Id = "deutschlandfunk",
            Name = "Deutschlandfunk",
            StreamUrl = "https://example.com/streams/deutschlandfunk.mp3",
            Country = "Alemania",
            Language = "Alemán",
            Tags = ["noticias", "cultura"],
            LogoUrl = "https://example.com/logos/deutschlandfunk.png"
        },
        new()
        {
            Id = "radio-italia",
            Name = "Radio Italia",
            StreamUrl = "https://example.com/streams/radio-italia.mp3",
            Country = "Italia",
            Language = "Italiano",
            Tags = ["música italiana", "pop"],
            LogoUrl = "https://example.com/logos/radio-italia.png"
        },
        new()
        {
            Id = "antena-1",
            Name = "Antena 1",
            StreamUrl = "https://example.com/streams/antena-1.mp3",
            Country = "Rumanía",
            Language = "Rumano",
            Tags = ["pop", "noticias"],
            LogoUrl = null
        },
        new()
        {
            Id = "nrk-p1",
            Name = "NRK P1",
            StreamUrl = "https://example.com/streams/nrk-p1.mp3",
            Country = "Noruega",
            Language = "Noruego",
            Tags = ["variedades", "actualidad"],
            LogoUrl = "https://example.com/logos/nrk-p1.png"
        }
    ];

    public Task<IReadOnlyList<StationDto>> GetStationsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Stations);
    }
}
