using System;
using System.Threading.Tasks;

namespace Meridian.Services.Images
{
    public interface IImageResolver
    {
        Task<Uri> GetArtistImageUri(string artist, bool big = true);

        Task<Uri> GetAlbumCover(string artist, string title);
    }
}