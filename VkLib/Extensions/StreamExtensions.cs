using System.Diagnostics;
using System.IO;

namespace VkLib.Extensions
{
    internal static class StreamExtensions
    {
        public static int CopyStream(this Stream source, Stream dest)
        {
            Debug.Assert(source != null);
            Debug.Assert(dest != null);
            var positionSource = source.Position;
            var positionDest = dest.Position;

            var buffer = new byte[4096];
            var read = 0;
            var total = 0;
            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                total += read;
                dest.Write(buffer, 0, read);
            }
            if (source.CanSeek)
                source.Seek(positionSource, SeekOrigin.Begin);
            if (dest.CanSeek)
                dest.Seek(positionDest, SeekOrigin.Begin);
            return total;
        }
    }
}
