using System.Runtime.CompilerServices;

namespace Jupiter.Core.Services.Settings
{
    public interface ISettingsService
    {
        void Set<T>(T value, [CallerMemberName] string key = null);

        T Get<T>([CallerMemberName] string key = null, T defaultValue = default(T));
    }
}
