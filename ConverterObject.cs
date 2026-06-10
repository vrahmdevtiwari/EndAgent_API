using System.IO;
using System.IO.Compression;
using System.Text;

namespace EndAgent_API.Common
{
    public class ConverterObject
    {
        public static string Decompress(byte[] compressedData)
        {
            using (var inputStream = new MemoryStream(compressedData))
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                gzipStream.CopyTo(outputStream);
                var decompressedBytes = outputStream.ToArray();
                return Encoding.UTF8.GetString(decompressedBytes);
            }
        }
    }
}
