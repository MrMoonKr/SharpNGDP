using SharpNGDP.Extensions;
using SharpNGDP.Files;
using SharpNGDP.Records;
using SharpNGDP.TACT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SharpNGDP.Managers
{
    /// <summary>
    /// 아카이브 패키지 파일 관리자
    /// </summary>
    public class ArchiveManager
    {
        public ArchiveManager( NGDPClient ngdpClient , CDNRecord cdn )
        {
            NGDPClient  = ngdpClient;
            CDN         = cdn;
        }

        private NGDPClient NGDPClient { get; }
        private CDNRecord CDN { get; }
        private Dictionary<string , ArchiveIndexFile> Indices { get; } = new Dictionary<string , ArchiveIndexFile>();
        private Dictionary<string , byte[]> Archives { get; } = new Dictionary<string , byte[]>();

        public void AddArchives( params string[] archiveFileHashes )
        {
            foreach ( var archiveFileHash in archiveFileHashes )
            {
                var res = NGDPClient.FileManager.Get( new TACTRequest( CDN, CDNRequestType.Data, $"{archiveFileHash}.index" ) );
                Indices.Add( archiveFileHash , res.GetFile<ArchiveIndexFile>() );
            }
        }

        public BLTEFile Get( string filehash )
        {
            foreach ( var kvp in Indices )
            {
                var archiveEntry = kvp.Value.Blocks.SelectMany( ab => ab.Entries ).FirstOrDefault( ae => ae.EKey.ToHexString() == filehash );
                if ( archiveEntry == null )
                    continue;

                // Lazily load archives
                if ( !Archives.TryGetValue( kvp.Key , out var archiveFile ) )
                {
                    var res = NGDPClient.FileManager.Get(new TACTRequest(CDN, CDNRequestType.Data, kvp.Key));
                    archiveFile = res.Buffer;
                    Archives.Add( kvp.Key , archiveFile );
                }

                var slice = new byte[BitConverter.ToUInt32(archiveEntry.CompressedSize, 0)];
                Array.Copy( archiveFile , BitConverter.ToUInt32( archiveEntry.BLTEChunkOffset , 0 ) , slice , 0 , slice.Length );
                using ( var ms = new MemoryStream( slice ) )
                {
                    var file = new BLTEFile(ms);
                    file.Read();
                    return file;
                }
            }

            return null;
        }
    }
}
