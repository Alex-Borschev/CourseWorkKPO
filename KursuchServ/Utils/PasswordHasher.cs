using System;
using System.Security.Cryptography;
using System.Text;

namespace Server { 
    public static class PasswordHasher
    {
        // Settings - you can increase the iterations for greater stability.
        private const int SaltSize = 16;      // 128 bit
        private const int HashSize = 32;      // 256 bit
        private const int Iterations = 150_000;

        // Returns a string that is safe to store in the database.
        public static string HashPassword(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            // Generating salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            // Getting the hash
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Generate a string: iterations.salt.hash (Base64)
                string result = $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
                return result;
            }
        }

        // Checks the password against the stored hash
        public static bool VerifyPassword(string password, string stored)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (stored == null) throw new ArgumentNullException(nameof(stored));

            // Analyze the format
            var parts = stored.Split('.', 3);
            if (parts.Length != 3) return false;

            if (!int.TryParse(parts[0], out int iterations)) return false;
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] storedHash = Convert.FromBase64String(parts[2]);

            // Calculate the hash for the input password
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] computedHash = pbkdf2.GetBytes(storedHash.Length);

                // Protected comparison (fixed time)
                return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
            }
        }
    }
}