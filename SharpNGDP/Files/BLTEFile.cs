using SharpNGDP.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SharpNGDP.Files
{
    public class BLTEFile : NGDPFile
    {
        private static readonly Logger log = Logger.Create<BLTEFile>();

        public BLTEFile(Stream stream)
            : base(stream)
        { }

        public BLTEHeader BLTEHeader { get; private set; }
        public BLTEChunkInfo BLTEChunkInfo { get; private set; }
        public BLTEDataBlock[] BLTEData { get; private set; }

        public byte[] Buffer { get; private set; }

        public override Stream GetStream() =>
            new MemoryStream(Buffer);

        public override void Read()
        {
            var sw = Stopwatch.StartNew();
            using (var br = new BinaryReader(base.GetStream()))
            {
                BLTEHeader = new BLTEHeader();
                BLTEHeader.Read(br);

                if (BLTEHeader.HeaderSize > 0)
                {
                    BLTEChunkInfo = new BLTEChunkInfo();
                    BLTEChunkInfo.Read(br);
                }


                BLTEData = new BLTEDataBlock[BLTEChunkInfo?.ChunkCount ?? 1];
                using (var ms = new MemoryStream())
                using (var bw = new BinaryWriter(ms))
                {
                    for (var i = 0; i < BLTEData.Length; i++)
                    {
                        BLTEData[i] = new BLTEDataBlock();
                        BLTEData[i].Read(br, BLTEChunkInfo.Chunks[i].CompressedSize);

                        bw.Write(ReadDatablock(BLTEChunkInfo.Chunks[i], BLTEData[i]));
                    }

                    Buffer = ms.ToArray();
                }
            }
            sw.Stop();
            log.WriteLine($"Parsed BLTEFile in {sw.Elapsed}");
            log.WriteLine($"Decompressed size {Buffer.Length} bytes");
        }

        private byte[] ReadDatablock(BLTEChunkInfoEntry chunkInfoEntry, BLTEDataBlock data)
        {
            using (var ms = new MemoryStream())
            {
                switch (data.EncodingMode)
                {
                    case 'N': // None
                        ms.Write(data.Data, 0, data.Data.Length);
                        break;

                    case 'Z': // Zlib
                        using (var cs = new MemoryStream(data.Data, 2, (int)chunkInfoEntry.CompressedSize - 3))
                        using (var ds = new DeflateStream(cs, CompressionMode.Decompress))
                            ds.CopyTo(ms);
                        break;

                    // TODO: Implement this
                    case 'E': // Encrypted
                    case 'F': // Frame
                    default:
                        throw new NotImplementedException();
                }

                if (ms.Length != chunkInfoEntry.DecompressedSize)
                    throw new Exception($"Decoded BLTE Data Block does not match DecompressedSize! Expected {chunkInfoEntry.DecompressedSize} actual {ms.Length}");

                return ms.ToArray();
            }
        }
    }

    public class BLTEHeader
    {
        public string Signature { get; private set; }
        public uint HeaderSize { get; private set; }

        public void Read(BinaryReader br)
        {
            Signature = Encoding.UTF8.GetString(br.ReadBytes(4));
            HeaderSize = br.ReadUInt32().SwapEndian();
        }
    }

    public class BLTEChunkInfo
    {
        public byte Flags { get; private set; }
        public uint ChunkCount { get; private set; }
        public BLTEChunkInfoEntry[] Chunks { get; private set; }

        public void Read(BinaryReader br)
        {
            // Do bit magic to extract 8 and 24 bit from 32 bit
            var tmp = br.ReadUInt32().SwapEndian();
            Flags = (byte)(tmp >> 24);
            ChunkCount = tmp & 0xFFF;
            Chunks = new BLTEChunkInfoEntry[ChunkCount];
            for (var i = 0; i < Chunks.Length; i++)
            {
                Chunks[i] = new BLTEChunkInfoEntry();
                Chunks[i].Read(br);
            }
        }
    }

    public class BLTEChunkInfoEntry
    {
        public uint CompressedSize { get; private set; }
        public uint DecompressedSize { get; private set; }
        public byte[] Checksum { get; private set; }

        public void Read(BinaryReader br)
        {
            CompressedSize = br.ReadUInt32().SwapEndian();
            DecompressedSize = br.ReadUInt32().SwapEndian();
            const int CHECKSUM_SIZE = 16;
            Checksum = br.ReadBytes(CHECKSUM_SIZE);
        }
    }

    public class BLTEDataBlock
    {
        public char EncodingMode { get; private set; }
        public byte[] Data { get; private set; }

        public void Read(BinaryReader br, uint compressedSize)
        {
            EncodingMode = br.ReadChar();
            Data = br.ReadBytes((int)compressedSize - 1);
        }
    }
}
