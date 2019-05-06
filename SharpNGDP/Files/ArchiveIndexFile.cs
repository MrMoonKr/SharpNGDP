using SharpNGDP.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpNGDP.Files
{
    public class ArchiveIndexFile : NGDPFile
    {
        private static Logger log = Logger.Create<ArchiveIndexFile>();

        public ArchiveIndexFile(Stream stream)
            : base(stream)
        { }

        public ArchiveIndexFooter Footer { get; private set; }
        public ArchiveIndexBlock[] Blocks { get; private set; }
        public ArchiveIndexContents Contents { get; private set; }

        public override void Read()
        {
            //base.Read();

            using (var br = new BinaryReader(GetStream()))
            {
                br.BaseStream.Seek(-ArchiveIndexFooter.FOOTER_SIZE, SeekOrigin.End);
                Footer = new ArchiveIndexFooter();
                Footer.Read(br);
                br.BaseStream.Seek(0, SeekOrigin.Begin);

                var totalBlocks = (br.BaseStream.Length - ArchiveIndexFooter.FOOTER_SIZE) / (Footer.BlockSize << 10);
                Blocks = new ArchiveIndexBlock[totalBlocks];
                for (var i = 0; i < Blocks.Length; i++)
                {
                    Blocks[i] = new ArchiveIndexBlock();
                    Blocks[i].Read(br, Footer);
                }

                Contents = new ArchiveIndexContents();
                Contents.Read(br, Footer, Blocks.Length);
            }
        }
    }

    public class ArchiveIndexEntry
    {
        public byte[] EKey { get; private set; }
        public byte[] CompressedSize { get; private set; }
        public byte[] BLTEChunkOffset { get; private set; }

        public void Read(BinaryReader br, ArchiveIndexFooter footer)
        {
            EKey = br.ReadBytes(footer.KeySize);
            CompressedSize = br.ReadBytes(footer.SizeBytes).ReverseBytes();
            BLTEChunkOffset = br.ReadBytes(footer.OffsetBytes).ReverseBytes();
        }
    }

    public class ArchiveIndexBlock
    {
        public ArchiveIndexEntry[] Entries { get; private set; }
        //public byte[] Padding { get; private set; }

        public void Read(BinaryReader br, ArchiveIndexFooter footer)
        {
            var sizeIndexEntry = footer.KeySize + footer.SizeBytes + footer.OffsetBytes;
            var blockSize = (footer.BlockSize << 10);
            Entries = new ArchiveIndexEntry[blockSize / sizeIndexEntry];
            for (var i = 0; i < Entries.Length; i++)
            {
                Entries[i] = new ArchiveIndexEntry();
                Entries[i].Read(br, footer);
            }

            // Either read padding
            //Padding = br.ReadBytes(blockSize - (Entries.Length * sizeIndexEntry));
            // or just skip it
            br.BaseStream.Seek(blockSize - (Entries.Length * sizeIndexEntry), SeekOrigin.Current);
        }
    }

    public class ArchiveIndexContents
    {
        public byte[][] Entries { get; private set; }
        public byte[][] Checksums { get; private set; }

        public void Read(BinaryReader br, ArchiveIndexFooter footer, int blockCount)
        {
            Entries = new byte[blockCount][];
            for (var i = 0; i < Entries.Length; i++)
                Entries[i] = br.ReadBytes(footer.KeySize);

            Checksums = new byte[blockCount][];
            for (var i = 0; i < Checksums.Length; i++)
                Checksums[i] = br.ReadBytes(footer.ChecksumSize);
        }
    }

    public class ArchiveIndexFooter
    {
        public const int FOOTER_SIZE = 0x1C;
        private const int CHECKSUM_SIZE = 0x08;

        public byte[] ContentHash { get; private set; }
        public byte Version { get; private set; }
        public byte unk0 { get; private set; }
        public byte unk1 { get; private set; }
        public byte BlockSize { get; private set; } // in kb
        public byte OffsetBytes { get; private set; }
        public byte SizeBytes { get; private set; }
        public byte KeySize { get; private set; }
        public byte ChecksumSize { get; private set; }
        public uint ElementCount { get; private set; }
        public byte[] FooterChecksum { get; private set; }

        public void Read(BinaryReader br)
        {
            ContentHash = br.ReadBytes(CHECKSUM_SIZE);
            Version = br.ReadByte();
            unk0 = br.ReadByte();
            unk1 = br.ReadByte();
            BlockSize = br.ReadByte();
            OffsetBytes = br.ReadByte();
            SizeBytes = br.ReadByte();
            KeySize = br.ReadByte();
            ChecksumSize = br.ReadByte();
            ElementCount = br.ReadUInt32();
            FooterChecksum = br.ReadBytes(CHECKSUM_SIZE);
        }
    }
}
