using System.Security.Cryptography;
using AIDIMS.Application.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AIDIMS.Infrastructure.Services;

/// <summary>
/// Service for hashing and verifying passwords using PBKDF2
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 128 / 8; // 128 bits
    private const int KeySize = 256 / 8; // 256 bits
    private const int Iterations = 10000;
    private const char Delimiter = ';';

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize
        );

        return string.Join(
            Delimiter,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash)
        );
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        if (string.IsNullOrEmpty(passwordHash))
            throw new ArgumentNullException(nameof(passwordHash));

        var elements = passwordHash.Split(Delimiter);
        if (elements.Length != 2)
            return false;

        var salt = Convert.FromBase64String(elements[0]);
        var hash = Convert.FromBase64String(elements[1]);

        var hashToCompare = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize
        );

        return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
    }
}
