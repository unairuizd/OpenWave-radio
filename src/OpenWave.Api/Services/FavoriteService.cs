using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using OpenWave.Api.Data.Entities;
using OpenWave.Api.Repositories;
using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// Favourites stored as a copy of the station data, so they work without the live catalogue.
/// </summary>
public sealed class FavoriteService(
    IUserRepository userRepository,
    IFavoriteRepository favoriteRepository) : IFavoriteService
{
    // Column sizes from database/schema.sql.
    private const int MaxStationIdLength = 64;
    private const int MaxNameLength = 255;
    private const int MaxUrlLength = 2048;
    private const int MaxCountryLength = 100;
    private const int MaxLanguageLength = 100;
    private const int MaxTagsLength = 500;

    private const int DuplicateKeyErrorNumber = 1062;
    private const int MissingParentRowErrorNumber = 1452;

    // Keys of Resources/Messages.resx: the controller answers with the text in the request language.
    private const string UnknownUserMessage = "UnknownUser";
    private const string InvalidStationMessage = "InvalidStation";
    private const string AlreadyFavoriteMessage = "AlreadyFavorite";
    private const string NotFavoriteMessage = "NotFavorite";

    public async Task<ServiceResult<IReadOnlyList<StationDto>>> ListAsync(long userId, CancellationToken cancellationToken)
    {
        if (!await userRepository.ExistsAsync(userId, cancellationToken))
        {
            return ServiceResult<IReadOnlyList<StationDto>>.Failure(ServiceError.NotFound, UnknownUserMessage);
        }

        var favorites = await favoriteRepository.ListByUserAsync(userId, cancellationToken);
        return ServiceResult<IReadOnlyList<StationDto>>.Success(favorites.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<StationDto>> AddAsync(long userId, StationDto station, CancellationToken cancellationToken)
    {
        if (!IsValid(station))
        {
            return ServiceResult<StationDto>.Failure(ServiceError.Invalid, InvalidStationMessage, "Station");
        }

        if (!await userRepository.ExistsAsync(userId, cancellationToken))
        {
            return ServiceResult<StationDto>.Failure(ServiceError.NotFound, UnknownUserMessage);
        }

        if (await favoriteRepository.ExistsAsync(userId, station.Id, cancellationToken))
        {
            return ServiceResult<StationDto>.Failure(ServiceError.Conflict, AlreadyFavoriteMessage);
        }

        var favorite = new FavoriteStationEntity
        {
            UserId = userId,
            StationUuid = station.Id,
            Name = Shorten(station.Name.Trim(), MaxNameLength)!,
            StreamUrl = station.StreamUrl.Trim(),
            LogoUrl = EmptyToNull(station.LogoUrl),
            Country = Shorten(EmptyToNull(station.Country), MaxCountryLength),
            Language = Shorten(EmptyToNull(station.Language), MaxLanguageLength),
            Tags = JoinTags(station.Tags),
            CreatedAt = DateTime.UtcNow,
        };

        try
        {
            await favoriteRepository.AddAsync(favorite, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: DuplicateKeyErrorNumber })
        {
            // An identical request was stored between the check and the insert.
            return ServiceResult<StationDto>.Failure(ServiceError.Conflict, AlreadyFavoriteMessage);
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: MissingParentRowErrorNumber })
        {
            // The user was deleted between the check and the insert.
            return ServiceResult<StationDto>.Failure(ServiceError.NotFound, UnknownUserMessage);
        }

        return ServiceResult<StationDto>.Success(ToDto(favorite));
    }

    public async Task<ServiceResult<bool>> RemoveAsync(long userId, string stationId, CancellationToken cancellationToken)
    {
        if (await favoriteRepository.RemoveAsync(userId, stationId, cancellationToken))
        {
            return ServiceResult<bool>.Success(true);
        }

        var message = await userRepository.ExistsAsync(userId, cancellationToken) ? NotFavoriteMessage : UnknownUserMessage;
        return ServiceResult<bool>.Failure(ServiceError.NotFound, message);
    }

    private static bool IsValid(StationDto? station)
    {
        return station is not null
            && !string.IsNullOrWhiteSpace(station.Id)
            && station.Id.Length <= MaxStationIdLength
            && !string.IsNullOrWhiteSpace(station.Name)
            && !string.IsNullOrWhiteSpace(station.StreamUrl)
            && station.StreamUrl.Trim().Length <= MaxUrlLength
            && (station.LogoUrl?.Trim().Length ?? 0) <= MaxUrlLength;
    }

    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Shorten(string? value, int maxLength) =>
        value is not null && value.Length > maxLength ? value[..maxLength].TrimEnd() : value;

    /// <summary>
    /// Joins the tags with commas, dropping whole tags from the end until the text fits the column.
    /// </summary>
    private static string? JoinTags(string[]? tags)
    {
        var kept = new List<string>();
        var length = 0;
        foreach (var tag in tags ?? [])
        {
            var value = tag?.Trim();
            if (string.IsNullOrEmpty(value) || value.Contains(','))
            {
                continue;
            }

            var added = kept.Count == 0 ? value.Length : value.Length + 1;
            if (length + added > MaxTagsLength)
            {
                break;
            }

            kept.Add(value);
            length += added;
        }

        return kept.Count > 0 ? string.Join(',', kept) : null;
    }

    private static StationDto ToDto(FavoriteStationEntity favorite)
    {
        var tags = (favorite.Tags ?? string.Empty)
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        return new StationDto
        {
            Id = favorite.StationUuid,
            Name = favorite.Name,
            StreamUrl = favorite.StreamUrl,
            Country = favorite.Country,
            Language = favorite.Language,
            Tags = tags.Length > 0 ? tags : null,
            LogoUrl = favorite.LogoUrl,
        };
    }
}
