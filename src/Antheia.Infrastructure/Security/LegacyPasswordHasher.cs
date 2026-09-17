using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Antheia.Infrastructure.Security
{
    public static class LegacyPasswordHasher
    {
        public static bool VerifyPassword(string password, string storedHash, string storedSalt, int passwordFormat)
        {
            return passwordFormat switch
            {
                // Format 0: Clear / Plaintext
                0 => string.Equals(password, storedHash, StringComparison.Ordinal),

                // Format 1: Hashed (HMACSHA1 with salt)
                1 => VerifyHashedPassword(password, storedHash, storedSalt),

                // Format 2: Encrypted (Requires MachineKey decryption if used)
                _ => throw new NotSupportedException($"Password format {passwordFormat} is not currently supported.")
            };
        }

        private static bool VerifyHashedPassword(string password, string storedHash, string storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);

            using var hmac = new HMACSHA1(saltBytes);
            byte[] computedHashBytes = hmac.ComputeHash(passwordBytes);
            string computedHash = Convert.ToBase64String(computedHashBytes);

            return string.Equals(storedHash, computedHash, StringComparison.Ordinal);
        }
    }
}
