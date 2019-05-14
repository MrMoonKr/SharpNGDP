using SharpNGDP.Ribbit;
using SharpNGDP.TACT;
using System.IO;

namespace SharpNGDP.Managers
{
    public class FileManager
    {
        public FileManager(NGDPClient ngdpClient, string basePath)
        {
            NGDPClient = ngdpClient;
            BasePath = basePath;
        }

        private NGDPClient NGDPClient { get; }
        public string BasePath { get; }

        public NGDPResponse Get(NGDPRequest request)
        {
            switch (request)
            {
                case RibbitRequest req:
                    {
                        var res = NGDPClient.RibbitClient.Execute(req);
                        var fp = Path.Combine(BasePath, "ribbit", res.GetFile<RibbitFile>().Checksum);
                        if (!File.Exists(fp))
                            WriteStreamToFile(res.GetStream(), fp);
                        return res;
                    }

                case TACTRequest req:
                    {
                        var fp = Path.Combine(BasePath, "cdn", req.URI.AbsolutePath.TrimStart('/'));
                        if (File.Exists(fp))
                            using (var fs = new FileStream(fp, FileMode.Open, FileAccess.Read))
                                return new TACTResponse(req, fs);

                        var res = NGDPClient.TACTClient.Get(req);
                        WriteStreamToFile(res.GetStream(), fp);

                        return res;
                    }

                default:
                    return null;
            }
        }

        private static void WriteStreamToFile(Stream stream, string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                stream.CopyTo(fs);
        }
    }
}
