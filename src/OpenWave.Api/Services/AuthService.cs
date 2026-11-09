using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using OpenWave.Api.Data.Entities;
using OpenWave.Api.Repositories;
using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// Registration and login with normalised emails and hashed passwords.
/// </summary>
public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher<UserEntity> passwordHasher) : IAuthService
{
    private const int MaxEmailLength = 255;
    private const int MinPasswordLength = 8;
    private const int DuplicateKeyErrorNumber = 1062;

    // Keys of Resources/Messages.resx: the controller answers with the text in the request language.
    private const string InvalidEmailMessage = "InvalidEmail";
    private const string ShortPasswordMessage = "ShortPassword";
    private const string EmailTakenMessage = "EmailTaken";
    private const string WrongCredentialsMessage = "WrongCredentials";

    public async Task<ServiceResult<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (!IsValidEmail(email))
        {
            return ServiceResult<UserDto>.Failure(ServiceError.Invalid, InvalidEmailMessage, nameof(request.Email));
        }

        var password = request.Password ?? string.Empty;
        if (password.Length < MinPasswordLength)
        {
            return ServiceResult<UserDto>.Failure(ServiceError.Invalid, ShortPasswordMessage, nameof(request.Password));
        }

        if (await userRepository.FindByEmailAsync(email, cancellationToken) is not null)
        {
            return ServiceResult<UserDto>.Failure(ServiceError.Conflict, EmailTakenMessage);
        }

        var user = new UserEntity { Email = email, PasswordHash = string.Empty };
        user.PasswordHash = passwordHasher.HashPassword(user, password);

        try
        {
            await userRepository.AddAsync(user, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: DuplicateKeyErrorNumber })
        {
            // Another request registered the same email between the check and the insert.
            return ServiceResult<UserDto>.Failure(ServiceError.Conflict, EmailTakenMessage);
        }

        return ServiceResult<UserDto>.Success(new UserDto(user.Id, user.Email));
    }

    public async Task<ServiceResult<UserDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (!IsValidEmail(email))
        {
            return ServiceResult<UserDto>.Failure(ServiceError.Invalid, InvalidEmailMessage, nameof(request.Email));
        }

        // Unknown email and wrong password give the same answer.
        var user = await userRepository.FindByEmailAsync(email, cancellationToken);
        if (user is null || string.IsNullOrEmpty(request.Password))
        {
            return ServiceResult<UserDto>.Failure(ServiceError.Unauthorized, WrongCredentialsMessage);
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return ServiceResult<UserDto>.Failure(ServiceError.Unauthorized, WrongCredentialsMessage);
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            await userRepository.SaveChangesAsync(cancellationToken);
        }

        return ServiceResult<UserDto>.Success(new UserDto(user.Id, user.Email));
    }

    private static string NormalizeEmail(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();

    private static bool IsValidEmail(string email)
    {
        // The parsed address must equal the input, which rejects forms like "Ana <ana@example.com>".
        return email.Length is > 0 and <= MaxEmailLength
            && MailAddress.TryCreate(email, out var address)
            && address.Address == email;
    }
}
