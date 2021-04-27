using System;
using System.IO;

namespace Jupiter.Utils.Extensions
{
    public static class StreamExtensions
    {
        public static void WriteText(this Stream stream, string text)
        {
            if (!stream.CanWrite)
                throw new Exception("Stream is not writeable.");

            using (var writer = new StreamWriter(stream))
            {
                writer.Write(text);
                writer.Flush();
            }
        }

        public static string ReadText(this Stream stream)
        {
            if (!stream.CanRead)
                throw new Exception("Stream is not readable.");

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
