using System.Net.Mail;

namespace OpenWave.App.Services;

/// <summary>
/// The same email and password rules the API applies, checked before sending.
/// </summary>
public static class AccountRules
{
    public const int MinPasswordLength = 8;
    private const int MaxEmailLength = 255;

    public static string NormalizeEmail(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();

    public static bool IsValidEmail(string normalizedEmail)
    {
        return normalizedEmail.Length is > 0 and <= MaxEmailLength
            && MailAddress.TryCreate(normalizedEmail, out var address)
            && address.Address == normalizedEmail;
    }
}
