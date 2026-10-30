using OpenWave.Api.Data.Entities;

namespace OpenWave.Api.Repositories;

/// <summary>
/// Data access for the <c>users</c> table.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Finds a user by an already normalised (trimmed, lower-case) email; the result is tracked so it can be updated.
    /// </summary>
    Task<UserEntity?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(long userId, CancellationToken cancellationToken);

    /// <summary>
    /// Inserts the user and saves; <see cref="UserEntity.Id"/> and <see cref="UserEntity.CreatedAt"/> are filled in.
    /// </summary>
    Task AddAsync(UserEntity user, CancellationToken cancellationToken);

    /// <summary>
    /// Saves changes made to tracked users (for example a rehashed password).
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
