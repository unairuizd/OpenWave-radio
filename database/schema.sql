-- =============================================================================
-- OpenWave database schema
--
-- Source of truth for the database structure. Run it manually in MySQL
-- Workbench, connected with the account provided by the teacher. The API never
-- creates, alters or drops database objects: no EF migrations, no EnsureCreated.
--
-- The database `2DAM_alumno19` already exists on the server, so this script only
-- selects it and creates the tables. Running it again is safe: existing tables
-- are left untouched.
--
-- Character set: utf8mb4 with the server's default collation (case-insensitive),
-- so the script works on MySQL 5.7 and 8.x.
-- =============================================================================

USE 2DAM_alumno19;

-- -----------------------------------------------------------------------------
-- Registered users. Emails are stored trimmed and in lower case by the API.
-- password_hash holds a salted hash, never the plain password.
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `users` (
    `id`            BIGINT       NOT NULL AUTO_INCREMENT,
    `email`         VARCHAR(255) NOT NULL,
    `password_hash` VARCHAR(255) NOT NULL,
    `created_at`    DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_users_email` (`email`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

-- -----------------------------------------------------------------------------
-- Favourite stations per user. A copy of the station data is stored so that
-- favourites keep working if the live catalogue changes. created_at is set by
-- the API (UTC) and orders the list, newest first.
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `favorite_stations` (
    `id`           BIGINT        NOT NULL AUTO_INCREMENT,
    `user_id`      BIGINT        NOT NULL,
    `station_uuid` VARCHAR(64)   NOT NULL,
    `name`         VARCHAR(255)  NOT NULL,
    `stream_url`   VARCHAR(2048) NOT NULL,
    `logo_url`     VARCHAR(2048) NULL,
    `country`      VARCHAR(100)  NULL,
    `language`     VARCHAR(100)  NULL,
    `tags`         VARCHAR(500)  NULL,
    `created_at`   DATETIME(6)   NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_favorite_stations_user_station` (`user_id`, `station_uuid`),
    CONSTRAINT `fk_favorite_stations_users`
        FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
        ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

-- -----------------------------------------------------------------------------
-- OPTIONAL: restricted account for the API (only SELECT, INSERT, UPDATE, DELETE).
--
-- On a shared school server your account will probably not be allowed to create
-- users; if these statements fail, skip them and let the API use the account
-- the teacher gave you. To use it, remove the comment marks, replace
-- <CONTRASEÑA_SEGURA> with a strong password and run only this block.
-- -----------------------------------------------------------------------------
-- CREATE USER IF NOT EXISTS 'openwave_app'@'%' IDENTIFIED BY '<CONTRASEÑA_SEGURA>';
-- GRANT SELECT, INSERT, UPDATE, DELETE ON `2DAM_alumno19`.* TO 'openwave_app'@'%';
