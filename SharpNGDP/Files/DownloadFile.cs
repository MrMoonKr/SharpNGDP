using SharpNGDP.Extensions;
using System;
using System.IO;
using System.Text;

namespace SharpNGDP.Files
{
    public class DownloadFile : BLTEFile
    {
        public DownloadFile(Stream stream)
            : base(stream)
        { }

        public DownloadHeader DownloadHeader { get; private set; }
        public DownloadEntry[] DownloadEntries { get; private set; }
        public DownloadTag[] DownloadTags { get; private set; }

        public override void Read()
        {
            base.Read();

            using (var br = new BinaryReader(GetStream()))
            {
                DownloadHeader = new DownloadHeader();
                DownloadHeader.Read(br);

                DownloadEntries = new DownloadEntry[DownloadHeader.EntryCount];
                for (var i = 0; i < DownloadEntries.Length; i++)
                {
                    DownloadEntries[i] = new DownloadEntry();
                    DownloadEntries[i].Read(br);
                }

                const decimal CHAR_BIT = 8.0M;
                var flagSize = (int)Math.Ceiling(DownloadHeader.EntryCount / CHAR_BIT);
                DownloadTags = new DownloadTag[DownloadHeader.TagCount];
                for (var i = 0; i < DownloadTags.Length; i++)
                {
                    DownloadTags[i] = new DownloadTag();
                    DownloadTags[i].Read(br, flagSize);
                }
            }
        }
    }

    public class DownloadHeader
    {
        public string Signature { get; private set; }
        public byte Version { get; private set; }
        public byte ChecksumSize { get; private set; }
        public byte unk0 { get; private set; }
        public uint EntryCount { get; private set; }
        public ushort TagCount { get; private set; }

        public void Read(BinaryReader br)
        {
            Signature = Encoding.UTF8.GetString(br.ReadBytes(2));
            Version = br.ReadByte();
            ChecksumSize = br.ReadByte();
            unk0 = br.ReadByte();
            EntryCount = br.ReadUInt32().SwapEndian();
            TagCount = br.ReadUInt16().SwapEndian();
        }
    }

    public class DownloadEntry
    {
        public byte unk0 { get; private set; }
        public byte[] Hash { get; private set; }
        public ulong FileSize { get; private set; }
        public byte DownloadPriority { get; private set; }
        public byte[] unk1 { get; private set; }

        public void Read(BinaryReader br)
        {
            unk0 = br.ReadByte();
            const int HASH_SIZE = 16;
            Hash = br.ReadBytes(HASH_SIZE);
            FileSize = br.ReadUInt40(true);
            DownloadPriority = br.ReadByte();
            unk1 = br.ReadBytes(4);
        }
    }

    public class DownloadTag
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
}
