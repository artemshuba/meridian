using System;
using System.Diagnostics;
#if MODERN
using Windows.Foundation.Collections;
using Windows.Storage;
#elif PHONE
using System.IO.IsolatedStorage;
#endif

namespace Neptune.Domain
{
    public class AppSettings
    {
#if MODERN
        private readonly IPropertySet _settings = ApplicationData.Current.LocalSettings.Values;
#elif PHONE
        private readonly IsolatedStorageSettings _settings = IsolatedStorageSettings.ApplicationSettings;
#elif DESKTOP
#endif

        protected void Set<TValue>(string settingName, TValue value)
        {
#if MODERN || PHONE
            try
            {
                var serializer = new Newtonsoft.Json.JsonSerializer();
                using (var writer = new System.IO.StringWriter())
                {
                    serializer.Serialize(writer, value);
                    _settings[settingName] = writer.GetStringBuilder().ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
#elif DESKTOP
            throw new NotImplementedException();
#endif
        }

        protected bool TryGet<TValue>(string settingName, out TValue value)
        {
#if MODERN || PHONE
            object val;
            if (_settings.TryGetValue(settingName, out val))
            {

                try
                {
                    var strValue = (string)val;
                    using (var reader = new System.IO.StringReader(strValue))
                    {
                        using (var jsonReader = new Newtonsoft.Json.JsonTextReader(reader))
                        {
                            var serializer = new Newtonsoft.Json.JsonSerializer();
                            value = serializer.Deserialize<TValue>(jsonReader);
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    value = default(TValue);
                    Debug.WriteLine(ex);
                }

                return false;
            }
            else
            {
                value = default(TValue);
                return false;
            }

#elif DESKTOP
            throw new NotImplementedException();
#endif
        }

        protected T Get<T>(string settingName, T defaultValue)
        {
            T obj;
            if (TryGet<T>(settingName, out obj))
                return obj;
            else
                return defaultValue;
        }

        public virtual void Load()
        {

        }

        public virtual void Save()
        {

        }
    }
}
