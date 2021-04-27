using Jupiter.Collections;
using System.Collections.Generic;
using Windows.Foundation.Collections;

namespace Jupiter.Application
{
    public interface IStateItems : IPropertySet
    {
        bool TryGet<T>(string key, out T value);

        T Get<T>(string key);
    }

    public class StateItems : ObservableDictionary<string, object>, IStateItems
    {
        public T Get<T>(string key)
        {
            object tryGetValue;
            if (TryGetValue(key, out tryGetValue))
            {
                return (T)tryGetValue;
            }
            throw new KeyNotFoundException();
        }

        public bool TryGet<T>(string key, out T value)
        {
            object tryGetValue;
            bool success = false;
            if (success = TryGetValue(key, out tryGetValue))
            {
                value = (T)tryGetValue;
            }
            else
            {
                value = default(T);
            }
            return success;
        }
    }
}