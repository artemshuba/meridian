using System.IO;
using System.Threading.Tasks;

namespace RemoteKit.Core
{
    public interface IRemoteKitApiClient
    {
        Task<string> ProcessCommandAsync(RemoteKitCommand command);

        Task<Stream> ProcessFileAsync(string targetPath);
    }
}