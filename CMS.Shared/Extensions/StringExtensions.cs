using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CMS.Shared.Extensions;

/// <summary>
/// Extension methods for string operations.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// Generates a SHA-256 hash of the input string.
    /// </summary>
    public static string ToSha256Hash(this string input)
    {
        ArgumentException.ThrowIfNullOrEmpty(input);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Generates a cryptographically secure random token.
    /// </summary>
    public static string GenerateSecureToken(int length = 64)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Checks if the string is a valid email address format.
    /// </summary>
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex().IsMatch(email);
    }

    /// <summary>
    /// Truncates the string to the specified maximum length.
    /// </summary>
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return string.Concat(value.AsSpan(0, maxLength - suffix.Length), suffix);
    }

    /// <summary>
    /// Converts a string to slug format (URL-friendly).
    /// </summary>
    public static string ToSlug(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Convert to lowercase
        var slug = value.ToLowerInvariant();

        // Replace spaces with hyphens
        slug = slug.Replace(' ', '-');

        // Remove invalid characters
        slug = SlugInvalidCharsRegex().Replace(slug, string.Empty);

        // Replace multiple hyphens with single hyphen
        slug = MultipleHyphensRegex().Replace(slug, "-");

        // Trim hyphens from start and end
        return slug.Trim('-');
    }

    /// <summary>
    /// Masks sensitive data in a string (e.g., email addresses).
    /// </summary>
    public static string MaskEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return email;

        var parts = email.Split('@');
        if (parts.Length != 2)
            return email;

        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
            return $"{localPart[0]}***@{domain}";

        return $"{localPart[0]}{new string('*', localPart.Length - 2)}{localPart[^1]}@{domain}";
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex SlugInvalidCharsRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleHyphensRegex();
}