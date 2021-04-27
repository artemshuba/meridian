using Windows.ApplicationModel.Resources;

namespace Meridian.Utils.Helpers
{
    public class Resources
    {
        private static readonly ResourceLoader R = new ResourceLoader();

        public string this[string index] => GetStringByKey(index);

        public static string GetStringByKey(string key)
        {
            return R?.GetString(key) ?? string.Empty;
        }
    }
}
