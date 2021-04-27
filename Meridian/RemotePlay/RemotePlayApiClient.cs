using System;
using System.IO;
using System.Threading.Tasks;
//using RemoteKit.Core;
using Meridian.Services;
using Windows.Storage;

namespace Meridian.RemotePlay
{
    //public class RemotePlayApiClient : IRemoteKitApiClient
    //{
    //    public Task<string> ProcessCommandAsync(RemoteKitCommand command)
    //    {
    //        switch (command.Command.ToLower())
    //        {
    //            case "player.play":
    //                AudioService.Instance.Play();
    //                break;

    //            case "player.pause":
    //                AudioService.Instance.Pause();
    //                break;

    //            case "player.next":
    //                AudioService.Instance.SwitchNext();
    //                break;

    //            case "player.prev":
    //                AudioService.Instance.SwitchPrev();
    //                break;
    //        }

    //        return Task.FromResult(string.Empty);
    //    }

    //    public async Task<Stream> ProcessFileAsync(string targetPath)
    //    {
    //        if (targetPath.StartsWith("/"))
    //        {
    //            //relative path, return content of RemotePlay/web folder
    //            var basePath = @"RemotePlay\web";
    //            var filePath = !string.IsNullOrWhiteSpace(targetPath) && targetPath.Length > 1 ? Path.Combine(basePath, Path.GetDirectoryName(targetPath.TrimStart('/', '\\'))) : Path.Combine(basePath, "index.html");

    //            try
    //            {
    //                var rootFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
    //                var folder = await rootFolder.GetFolderAsync(filePath);
    //                var file = await folder.TryGetItemAsync(Path.GetFileName(targetPath)) as StorageFile;
    //                if (file != null)
    //                {
    //                    return (await file.OpenReadAsync()).AsStreamForRead();
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                Logger.Error(ex);
    //            }
    //        }

    //        return null;
    //    }
    //}
}
