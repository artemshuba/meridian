using System.Collections.Generic;

namespace Jupiter.Mvvm
{
    //Dictionary wrapper because binding directly to Dictionary by the key from XAML won't work with .NET Native
    public class OperationTokenCollection
    {
        private Dictionary<string, OperationToken> _tokens = new Dictionary<string, OperationToken>();

        public void Add(string key, OperationToken value)
        {
            _tokens.Add(key, value);
        }

        public bool IsRegistered(string key)
        {
            return _tokens.ContainsKey(key);
        }

        public OperationToken this[string key]
        {
            get { return _tokens[key]; }
        }
    }
}