using System;
using Xunit;
using PasswordHasherLibrary;

namespace Server.Tests.ServicesTests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashPassword_ReturnsNonEmptyString()
        {
            string password = "mySecret123";

            string hash = PasswordHasher.HashPassword(password);

            Assert.False(string.IsNullOrEmpty(hash));
            Assert.NotEqual(password, hash);
        }

        [Fact]
        public void VerifyPassword_ReturnsTrue_ForCorrectPassword()
        {
            string password = "mySecret123";
            string hash = PasswordHasher.HashPassword(password);

            bool result = PasswordHasher.VerifyPassword(password, hash);

            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_ReturnsFalse_ForIncorrectPassword()
        {
            string password = "mySecret123";
            string hash = PasswordHasher.HashPassword(password);

            bool result = PasswordHasher.VerifyPassword("wrongPassword", hash);

            Assert.False(result);
        }

        [Fact]
        public void HashPassword_ProducesDifferentHashes_ForSamePassword()
        {
            string password = "mySecret123";

            string hash1 = PasswordHasher.HashPassword(password);
            string hash2 = PasswordHasher.HashPassword(password);

            Assert.NotEqual(hash1, hash2); // разные соли → разные хеши
        }

        [Fact]
        public void HashPassword_ThrowsArgumentNullException_ForNullPassword()
        {
            Assert.Throws<ArgumentNullException>(() => PasswordHasher.HashPassword(null!));
        }

        [Fact]
        public void VerifyPassword_ThrowsArgumentNullException_ForNullInputs()
        {
            string hash = PasswordHasher.HashPassword("test");

            Assert.Throws<ArgumentNullException>(() => PasswordHasher.VerifyPassword(null!, hash));
            Assert.Throws<ArgumentNullException>(() => PasswordHasher.VerifyPassword("test", null!));
        }

        [Fact]
        public void VerifyPassword_ReturnsFalse_ForInvalidFormatHash()
        {
            string invalidHash = "invalid.hash.format";

            bool result = PasswordHasher.VerifyPassword("password", invalidHash);

            Assert.False(result);
        }
    }
}
