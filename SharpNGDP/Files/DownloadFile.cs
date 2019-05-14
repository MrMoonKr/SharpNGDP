using SharpNGDP.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SharpNGDP.Files
{
    public class DownloadFile : BLTEFile
    {
        private static readonly Logger log = Logger.Create<DownloadFile>();

        public DownloadFile(Stream stream)
            : base(stream)
        { }

        public DownloadHeader DownloadHeader { get; private set; }
        public DownloadEntry[] DownloadEntries { get; private set; }
        public DownloadTag[] DownloadTags { get; private set; }

        public IEnumerable<DownloadEntry> GetEntriesWithTag(DownloadTag tag)
        {
            var fileBits = new BitArray(tag.Files);
            for (var i = 0; i < DownloadEntries.Length; i++)
                if (fileBits[i])
                    yield return DownloadEntries[i];
        }

        public override void Read()
        {
            base.Read();

            var sw = Stopwatch.StartNew();
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

                const byte CHAR_BIT = 8;
                var flagSize = (int)(DownloadHeader.EntryCount + (CHAR_BIT - 1)) / CHAR_BIT;
                DownloadTags = new DownloadTag[DownloadHeader.TagCount];
                for (var i = 0; i < DownloadTags.Length; i++)
                {
                    DownloadTags[i] = new DownloadTag();
                    DownloadTags[i].Read(br, flagSize);
                }
            }
            sw.Stop();
            log.WriteLine($"Parsed DownloadFile file in {sw.Elapsed}");
            log.WriteLine($"{DownloadEntries.Length} entries and {DownloadTags.Length} tags");
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
        public ulong Size { get; private set; }
        public byte DownloadPriority { get; private set; }
        public byte[] unk1 { get; private set; }

        public void Read(BinaryReader br)
        {
            unk0 = br.ReadByte();
            const int HASH_SIZE = 0x10;
            Hash = br.ReadBytes(HASH_SIZE);
            Size = br.ReadUInt40(true);
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
