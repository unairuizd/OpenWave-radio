using Microsoft.EntityFrameworkCore;
using OpenWave.Api.Data.Entities;

namespace OpenWave.Api.Data;

/// <summary>
/// EF Core access to the tables created by <c>database/schema.sql</c>.
/// The schema is owned by that script: this context never creates or migrates anything.
/// </summary>
public sealed class OpenWaveDbContext(DbContextOptions<OpenWaveDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<FavoriteStationEntity> FavoriteStations => Set<FavoriteStationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(user =>
        {
            user.ToTable("users");
            user.HasKey(u => u.Id);
            user.Property(u => u.Id).HasColumnName("id");
            user.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            user.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            user.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .ValueGeneratedOnAdd();
            user.HasIndex(u => u.Email).IsUnique().HasDatabaseName("ux_users_email");
        });

        modelBuilder.Entity<FavoriteStationEntity>(favorite =>
        {
            favorite.ToTable("favorite_stations");
            favorite.HasKey(f => f.Id);
            favorite.Property(f => f.Id).HasColumnName("id");
            favorite.Property(f => f.UserId).HasColumnName("user_id");
            favorite.Property(f => f.StationUuid).HasColumnName("station_uuid").HasMaxLength(64).IsRequired();
            favorite.Property(f => f.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            favorite.Property(f => f.StreamUrl).HasColumnName("stream_url").HasMaxLength(2048).IsRequired();
            favorite.Property(f => f.LogoUrl).HasColumnName("logo_url").HasMaxLength(2048);
            favorite.Property(f => f.Country).HasColumnName("country").HasMaxLength(100);
            favorite.Property(f => f.Language).HasColumnName("language").HasMaxLength(100);
            favorite.Property(f => f.Tags).HasColumnName("tags").HasMaxLength(500);
            favorite.Property(f => f.CreatedAt).HasColumnName("created_at");
            favorite.HasIndex(f => new { f.UserId, f.StationUuid })
                .IsUnique()
                .HasDatabaseName("ux_favorite_stations_user_station");
            favorite.HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_favorite_stations_users");
        });
    }
}
