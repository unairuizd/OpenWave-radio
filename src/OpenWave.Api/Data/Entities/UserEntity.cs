namespace OpenWave.Api.Data.Entities;

/// <summary>
/// A row of the <c>users</c> table.
/// </summary>
public sealed class UserEntity
{
    public long Id { get; set; }

    /// <summary>
    /// Trimmed, lower-case email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Salted password hash produced by <c>PasswordHasher</c>; never the plain password.
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Set by the database default when the row is inserted.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
