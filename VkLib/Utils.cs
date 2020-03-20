using System;
using System.Collections.Generic;
using System.Text;

namespace Sharp_VAG_Deluxe_3000 {
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
        ///     Build GET query URL.
        /// </summary>
        /// <param name="baseUrl">Base URL to add params to.</param>
        /// <param name="params">Params dictionary.</param>
        /// <returns>Query URL.</returns>
        public static string BuildUrl(string baseUrl, Dictionary<string, string> @params) {
            if (@params.Count <= 0) return baseUrl;
            baseUrl += "?";
            var isFirst = true;
            foreach (var param in @params) {
                if (string.IsNullOrWhiteSpace(param.Key) || string.IsNullOrEmpty(param.Value)) continue;

                if (!isFirst)
                    baseUrl += "&";
                else
                    isFirst = false;

                baseUrl += Uri.EscapeDataString(param.Key) + "=" + Uri.EscapeDataString(param.Value);
            }

            return baseUrl;
        }

        /// <summary>
        ///     Get enum instance from its value.
        /// </summary>
        /// <param name="value">One of source enum values.</param>
        /// <typeparam name="T">Source enum.</typeparam>
        /// <returns>Enum instance associated with provided value, null if no element associated with that value.</returns>
        public static T? GetEnumObjectByValue<T>(int? value) where T : struct {
            if (value == null) return null;
            if (Enum.IsDefined(typeof(T), value)) return (T) Enum.ToObject(typeof(T), value);
            return null;
        }

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
