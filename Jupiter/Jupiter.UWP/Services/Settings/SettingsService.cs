using System;
using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using Windows.Storage;
using Jupiter.Core.Services.Settings;
using Newtonsoft.Json;

namespace Jupiter.Services.Settings
{
    /// <summary>
    /// Service to load and store local settings
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private static ISettingsService _local;
        private static ISettingsService _roaming;

        /// <summary>
        /// Local settings. Stored on current device.
        /// </summary>
        public static ISettingsService Local => _local ?? (_local = new SettingsService(ApplicationData.Current.LocalSettings.Values));

        /// <summary>
        /// Roaming settings. Stored on user account and may be roamed (must be less than 100KB) to other devices of current user.
        /// </summary>
        public static ISettingsService Roaming => _roaming ?? (_roaming = new SettingsService(ApplicationData.Current.RoamingSettings.Values));

        protected IPropertySet Values { get; set; }

        private SettingsService(IPropertySet values)
        {
            Values = values;
        }

        /// <summary>
        /// Store value
        /// </summary>
        public void Set<T>(T value, [CallerMemberName]string key = null)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Key must not be null");

            var container = new ApplicationDataCompositeValue();
            var serializedValue = JsonConvert.SerializeObject(value);
            container["Value"] = serializedValue;
            Values[key] = container;
        }

        /// <summary>
        /// Load value
        /// </summary>
        public T Get<T>([CallerMemberName] string key = null, T defaultValue = default(T))
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Key must not be null");

            try
            {
                if (Values.ContainsKey(key))
                {
                    //if there is a container stored at specified key, unwrap it
                    var container = Values[key] as ApplicationDataCompositeValue;
                    if (container != null && container.ContainsKey("Value"))
                    {
                        var value = container["Value"] as string;
                        var converted = JsonConvert.DeserializeObject<T>(value);
                        return converted;
                    }
                    else
                    {
                        //else (e.g. updating old version of the app) there may be direct value, so just take it
                        if (Values[key].GetType() == typeof(T))
                            return (T)Values[key];
                    }
                }

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}