namespace Jupiter.Utils.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// Numerals localization
        /// </summary>
        public static string LocalizeNumerals(int value, string singularString, string dualString, string pluralString, string locale)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(locale) && locale.StartsWith("en"))
                return value == 1 ? singularString : pluralString;

            int plural = (value % 10 == 1 && value % 100 != 11 && value != 0 ? 0 : value % 10 >= 2 && value % 10 <= 4 && (value % 100 < 10 || value % 100 >= 20) && value != 0 ? 1 : 2);
            switch (plural)
            {
                case 0:
                    result = singularString;
                    break;
                case 1:
                    result = dualString;
                    break;
                case 2:
                    result = pluralString;
                    break;
            }

            return result;
        }
    }
}