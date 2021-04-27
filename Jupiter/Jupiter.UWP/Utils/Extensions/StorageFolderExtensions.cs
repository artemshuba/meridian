using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Jupiter.Utils.Extensions
{
    public static class StorageFolderExtensions
    {
        public static async Task<IStorageItem> TryGetItemAsync(this IStorageFolder folder, string name)
        {
            try
            {
                var item = await folder.GetItemAsync(name);
                return item;
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception)
            {

            }

            return null;
        }
    }
}
