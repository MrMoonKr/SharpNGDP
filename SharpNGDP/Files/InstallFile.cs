using SharpNGDP.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SharpNGDP.Files
{
    public class InstallFile : BLTEFile
    {
        private static Logger log = Logger.Create<InstallFile>();

        public InstallFile(Stream stream)
            : base(stream)
        { }

        public InstallFileHeader InstallFileHeader { get; private set; }
        public InstallFileTag[] InstallFileTags { get; private set; }
        public InstallFileEntry[] InstallFileEntries { get; private set; }

        public override void Read()
        {
            base.Read();

            var sw = Stopwatch.StartNew();
            using (var br = new BinaryReader(GetStream()))
            {
                InstallFileHeader = new InstallFileHeader();
                InstallFileHeader.Read(br);

                const decimal CHAR_BIT = 8.0M;
                var flagSize = (int)Math.Ceiling(InstallFileHeader.EntryCount / CHAR_BIT);

                InstallFileTags = new InstallFileTag[InstallFileHeader.TagCount];
                for (var i = 0; i < InstallFileTags.Length; i++)
                {
                    InstallFileTags[i] = new InstallFileTag();
                    InstallFileTags[i].Read(br, flagSize);
                }

                InstallFileEntries = new InstallFileEntry[InstallFileHeader.EntryCount];
                for (var i = 0; i < InstallFileEntries.Length; i++)
                {
                    InstallFileEntries[i] = new InstallFileEntry();
                    InstallFileEntries[i].Read(br, InstallFileHeader.HashSize);
                }
            }

            sw.Stop();
            log.WriteLine($"Parsed InstallFile in {sw.Elapsed}");
            log.WriteLine($"{InstallFileEntries.Length} entries and {InstallFileTags.Length} tags");
        }
    }

    public class InstallFileHeader
    {
        public string Signature { get; private set; }
        public byte Version { get; private set; }
        public byte HashSize { get; private set; }
        public ushort TagCount { get; private set; }
        public uint EntryCount { get; private set; }

        public void Read(BinaryReader br)
        {
            Signature = Encoding.UTF8.GetString(br.ReadBytes(2));
            Version = br.ReadByte();
            HashSize = br.ReadByte();
            TagCount = br.ReadUInt16().SwapEndian();
            EntryCount = br.ReadUInt32().SwapEndian();
        }
    }

    public class InstallFileTag
    {
        public string Name { get; private set; }
        public ushort Type { get; private set; }
        public byte[] Files { get; private set; }

        public void Read(BinaryReader br, int fileTagBytes)
        {
            Name = br.ReadCString();
            Type = br.ReadUInt16().SwapEndian();
            Files = br.ReadBytes(fileTagBytes);
        }
    }

    public class InstallFileEntry
    {
        public string Name { get; private set; }
        public byte[] Hash { get; private set; }
        public uint Size { get; private set; }

        public void Read(BinaryReader br, byte hashSize)
        {
            Name = br.ReadCString();
            Hash = br.ReadBytes(hashSize);
            Size = br.ReadUInt32().SwapEndian();
        }
    }
}
