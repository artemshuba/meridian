using System;
using System.Collections.Generic;
using System.Text;

namespace VkLib {
    /// <summary>
    ///     Various utilities.
    /// </summary>
    public static class Utils {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";

        /// <summary>
        ///     Generates a string consisting of random characters (from <see cref="Alphabet" />)
        /// </summary>
        /// <param name="length">Length of generated string.</param>
        /// <returns>A string consisting of random characters</returns>
        public static string GenerateRandomString(int length) {
            var random = new Random();
            var sb = new StringBuilder(length);
            for (var i = 0; i < length; i++) sb.Append(Alphabet[random.Next(Alphabet.Length)]);

            return sb.ToString();
        }

        /// <summary>
        ///     Creates compact integer representation called VarInt, as in Protobuf
        ///     (see <a href="https://developers.google.com/protocol-buffers/docs/encoding#varints">official docs</a>).
        /// </summary>
        /// <param name="value">An integer.</param>
        /// <returns>A byte array, representation of the input integer</returns>
        public static IEnumerable<byte> VarIntWrite(int value) {
            while (value != 0) {
                var current = value & 0x7F;
                value >>= 7;
                if (value != 0)
                    yield return (byte) (current | 0x80);
                else
                    yield return (byte) current;
            }
        }
    }
}
