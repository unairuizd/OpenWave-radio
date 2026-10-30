using Microsoft.EntityFrameworkCore;
using OpenWave.Api.Data;
using OpenWave.Api.Data.Entities;

namespace OpenWave.Api.Repositories;

public sealed class UserRepository(OpenWaveDbContext db) : IUserRepository
{
    public Task<UserEntity?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return db.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public Task<bool> ExistsAsync(long userId, CancellationToken cancellationToken)
    {
        return db.Users.AnyAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task AddAsync(UserEntity user, CancellationToken cancellationToken)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}
