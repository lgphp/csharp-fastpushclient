using System.IO;
using System.IO.Compression;
using System.Text;

namespace FastLivePushClient.Util
{
    public class GzipUtil
    {
        public static byte[] UnGzip(byte[] bytes)
        {
            string result;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (GZipStream decompressedStream = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (StreamReader sr = new StreamReader(decompressedStream, Encoding.UTF8))
                    {
                        result = sr.ReadToEnd();
                    }
                }
            }
            return Encoding.UTF8.GetBytes(result);
        }

    
        public static byte[] Gzip( byte[] rawData )
        {
            using (MemoryStream ms = new MemoryStream())
            {
                GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress, true);
                compressedzipStream.Write(rawData, 0, rawData.Length);
                compressedzipStream.Close();
                return ms.ToArray();
            }
        }
    }
}