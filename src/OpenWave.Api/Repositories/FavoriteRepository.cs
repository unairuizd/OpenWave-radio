using Microsoft.EntityFrameworkCore;
using OpenWave.Api.Data;
using OpenWave.Api.Data.Entities;

namespace OpenWave.Api.Repositories;

public sealed class FavoriteRepository(OpenWaveDbContext db) : IFavoriteRepository
{
    public async Task<IReadOnlyList<FavoriteStationEntity>> ListByUserAsync(long userId, CancellationToken cancellationToken)
    {
        return await db.FavoriteStations
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ThenByDescending(f => f.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(long userId, string stationUuid, CancellationToken cancellationToken)
    {
        return db.FavoriteStations.AnyAsync(f => f.UserId == userId && f.StationUuid == stationUuid, cancellationToken);
    }

    public async Task AddAsync(FavoriteStationEntity favorite, CancellationToken cancellationToken)
    {
        db.FavoriteStations.Add(favorite);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoveAsync(long userId, string stationUuid, CancellationToken cancellationToken)
    {
        var favorite = await db.FavoriteStations
            .SingleOrDefaultAsync(f => f.UserId == userId && f.StationUuid == stationUuid, cancellationToken);
        if (favorite is null)
        {
            return false;
        }

        db.FavoriteStations.Remove(favorite);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
