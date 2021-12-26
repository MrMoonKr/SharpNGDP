using SharpNGDP.Extensions;
using SharpNGDP.Files;
using SharpNGDP.Records;
using SharpNGDP.TACT;
using SharpNGDP.Managers;
using System;
using System.Linq;

namespace SharpNGDP
{
    public class VersionManager
    {
        public VersionManager( VersionRecord version , CDNRecord cdn )
        {
            NGDPClient = new NGDPClient();

            Version = version;
            CDN = cdn;
        }

        public NGDPClient NGDPClient { get; }

        public VersionRecord Version { get; }
        public CDNRecord CDN { get; }


        private ArchiveManager _archiveManager;
        public ArchiveManager ArchiveManager
        {
            get
            {
                if ( _archiveManager == null )
                {
                    _archiveManager = new ArchiveManager( NGDPClient , CDN );
                    _archiveManager.AddArchives( CDNConfig.Dictionary[ "archives" ].Split( ' ' ) );
                }
                return _archiveManager;
            }
        }

        private KeyValueFile _buildConfig;
        public KeyValueFile BuildConfig =>
            _buildConfig ?? ( _buildConfig = NGDPClient.FileManager.Get( new TACTRequest( CDN , CDNRequestType.Config , Version.BuildConfig ) ).GetFile<KeyValueFile>() );


        public bool HasEncoding =>
            BuildConfig.Dictionary.ContainsKey( "encoding" ) && 
            !string.IsNullOrEmpty( BuildConfig.Dictionary[ "encoding" ] ) && 
            BuildConfig.Dictionary[ "encoding" ].Split( ' ' ).Length == 2;

        private string EncodingFileHash =>
            GetEKey( BuildConfig.Dictionary[ "encoding" ].Split( ' ' ) );

        private EncodingFile _encondingFile;
        public EncodingFile EncodingFile =>
            _encondingFile
            ?? ( _encondingFile = NGDPClient.FileManager.Get( new TACTRequest( CDN , CDNRequestType.Data , EncodingFileHash ) ).GetFile<EncodingFile>() );


        private InstallFile _installFile;
        public InstallFile InstallFile =>
            _installFile
            ?? ( _installFile = NGDPClient.FileManager.Get( new TACTRequest( CDN , CDNRequestType.Data , GetEKey( BuildConfig.Dictionary[ "install" ].Split( ' ' ) ) ) ).GetFile<InstallFile>() );


        private DownloadFile _downloadFile;
        public DownloadFile DownloadFile => 
            _downloadFile ?? ( _downloadFile = NGDPClient.FileManager.Get( new TACTRequest( CDN , CDNRequestType.Data , GetEKey( BuildConfig.Dictionary[ "download" ].Split( ' ' ) ) ) )
                                                                     .GetFile<DownloadFile>() );


        private KeyValueFile _cdnConfig;
        public KeyValueFile CDNConfig =>
            _cdnConfig ?? 
            ( _cdnConfig = NGDPClient.FileManager.Get( new TACTRequest( CDN , CDNRequestType.Config , Version.CDNConfig ) )
                                                 .GetFile<KeyValueFile>() );


        private string[] ArchiveFileHashes =>
            CDNConfig.Dictionary[ "archives" ].Split( ' ' );

        private ArchiveIndexFile[] _archiveIndices;
        public ArchiveIndexFile[] ArchiveIndices =>
            _archiveIndices
            ?? ( _archiveIndices = ArchiveFileHashes.Select( h => NGDPClient.FileManager.Get( new TACTRequest( CDN , CDNRequestType.Data , $"{h}.index" ) ).GetFile<ArchiveIndexFile>() ).ToArray() );


        private string GetEKey( string[] input )
        {
            if ( input.Length > 1 )
                return input[ 1 ];

            var ekey = EncodingFile.ContentKeyEntries.FirstOrDefault(e => e.CKey.ToHexString() == input[0]);
            return ekey?.EKey[ 0 ]?.ToHexString();
        }

        public string GetEKeyByCKey( string ckey ) =>
            EncodingFile.ContentKeyEntries.FirstOrDefault( e => e.CKey.ToHexString() == ckey )?.EKey.FirstOrDefault().ToHexString();

        public System.IO.Stream RequestFile( string ckey )
        {
            var key = ckey;

            var ekey = GetEKeyByCKey(key);
            if ( !string.IsNullOrEmpty( ekey ) )
                key = ekey;

            var archiveFile = ArchiveManager.Get(key);
            if ( archiveFile != null )
            {
                return archiveFile.GetStream();
            }

            var req = new TACTRequest(CDN, CDNRequestType.Data, key);
            var res = NGDPClient.FileManager.Get(req);
            return res.GetFile<BLTEFile>().GetStream();
        }
    }
}
