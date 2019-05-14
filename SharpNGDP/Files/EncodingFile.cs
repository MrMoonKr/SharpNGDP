using SharpNGDP.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SharpNGDP.Files
{
    public class EncodingFile : BLTEFile
    {
        private static readonly Logger log = Logger.Create<EncodingFile>();

        public EncodingFile(Stream stream)
            : base(stream)
        { }

        public EncodingHeader EncodingHeader { get; private set; }
        public string[] ESpec { get; private set; }

        public EncodingTableIndex[] ContentKeyTableIndices { get; private set; }
        public ContentKeyEntry[] ContentKeyEntries { get; private set; }

        public EncodingTableIndex[] EKeyTableIndices { get; private set; }
        public EKeySpecEntry[] EKeySpecEntries { get; private set; }

        public string EncodingFileProfile { get; private set; }

        public override void Read()
        {
            base.Read();

            var sw = Stopwatch.StartNew();
            using (var br = new BinaryReader(GetStream()))
            {
                EncodingHeader = new EncodingHeader();
                EncodingHeader.Read(br);

                var espec = new List<string>();
                var startPos = br.BaseStream.Position;
                while (br.BaseStream.Position < startPos + (long)EncodingHeader.ESpecBlockSize)
                    espec.Add(br.ReadCString());
                ESpec = espec.ToArray();

                ContentKeyTableIndices = new EncodingTableIndex[EncodingHeader.CEKeyPageTablePageCount];
                
                for (var i = 0; i < ContentKeyTableIndices.Length; i++)
                {
                    ContentKeyTableIndices[i] = new EncodingTableIndex();
                    ContentKeyTableIndices[i].Read(br, EncodingHeader.CKeyHashSize);
                }

                var ckeys = new List<ContentKeyEntry>();
                for (var i = 0; i < EncodingHeader.CEKeyPageTablePageCount; i++)
                {
                    var offset = br.BaseStream.Position;
                    long remaining() => (offset + (EncodingHeader.CEKeyPageTableSize * 1024)) - br.BaseStream.Position;
                    while (remaining() > 0)
                    {
                        var entry = new ContentKeyEntry();
                        entry.Read(br, EncodingHeader.CKeyHashSize, EncodingHeader.EKeyHashSize);
                        if (entry.KeyCount == 0)
                            break;

                        ckeys.Add(entry);
                    }

                    br.BaseStream.Position = offset + EncodingHeader.CEKeyPageTableSize * 1024;
                }
                ContentKeyEntries = ckeys.ToArray();

                EKeyTableIndices = new EncodingTableIndex[EncodingHeader.EKeySpecTablePageCount];
                for (var i = 0; i < EKeyTableIndices.Length; i++)
                {
                    EKeyTableIndices[i] = new EncodingTableIndex();
                    EKeyTableIndices[i].Read(br, EncodingHeader.EKeyHashSize);
                }

                var ekeys = new List<EKeySpecEntry>();
                for (var i = 0; i < EncodingHeader.EKeySpecTablePageCount; i++)
                {
                    var offset = br.BaseStream.Position;
                    long remaining() => (offset + (EncodingHeader.EKeySpecPageTableSize * 1024)) - br.BaseStream.Position;
                    while (remaining() > 0)
                    {
                        var entry = new EKeySpecEntry();
                        entry.Read(br, EncodingHeader.EKeyHashSize);
                        //if (entry.ESpecIndex == -1)
                        //    break;

                        ekeys.Add(entry);
                    }

                    br.BaseStream.Position = offset + EncodingHeader.EKeySpecPageTableSize * 1024;
                }
                EKeySpecEntries = ekeys.ToArray();

                EncodingFileProfile = Encoding.UTF8.GetString(br.ReadBytes((int)br.Remaining()));
            }
            sw.Stop();
            log.WriteLine($"Parsed EncodingFile in {sw.Elapsed}");
            log.WriteLine($"{ContentKeyEntries.Length} CKeys and {EKeySpecEntries.Length} EKeys");
        }
    }

    public class EncodingHeader
    {
        public string Signature { get; private set; }

        public byte Version { get; private set; }

        public byte CKeyHashSize { get; private set; }
        public byte EKeyHashSize { get; private set; }

        public ushort CEKeyPageTableSize { get; private set; } // in kilobytes
        public ushort EKeySpecPageTableSize { get; private set; } // ^

        public uint CEKeyPageTablePageCount { get; private set; }
        public uint EKeySpecTablePageCount { get; private set; }

        public byte unk0 { get; private set; }

        public ulong ESpecBlockSize { get; private set; }

        public void Read(BinaryReader br)
        {
            Signature = Encoding.UTF8.GetString(br.ReadBytes(2));

            Version = br.ReadByte();

            CKeyHashSize = br.ReadByte();
            EKeyHashSize = br.ReadByte();

            CEKeyPageTableSize = br.ReadUInt16().SwapEndian();
            EKeySpecPageTableSize = br.ReadUInt16().SwapEndian();

            CEKeyPageTablePageCount = br.ReadUInt32().SwapEndian();
            EKeySpecTablePageCount = br.ReadUInt32().SwapEndian();

            ESpecBlockSize = br.ReadUInt40(true);
        }
    }

    public class EncodingTableIndex
    {
        public byte[] FirstHash { get; private set; }
        public byte[] Checksum { get; private set; }

        public void Read(BinaryReader br, uint hashSize)
        {
            FirstHash = br.ReadBytes((int)hashSize);
            const int CHECKSUM_SIZE = 16;
            Checksum = br.ReadBytes(CHECKSUM_SIZE);
        }
    }

    public class ContentKeyEntry
    {
        public byte KeyCount { get; private set; }
        public ulong FileSize { get; private set; }
        public byte[] CKey { get; private set; }
        public byte[][] EKey { get; private set; }

        public void Read(BinaryReader br, uint ckeyHashSize, uint ekeyHashSize)
        {
            KeyCount = br.ReadByte();
            FileSize = br.ReadUInt40(true);
            CKey = br.ReadBytes((int)ckeyHashSize);
            EKey = new byte[KeyCount][];
            for (var i = 0; i < EKey.Length; i++)
                EKey[i] = br.ReadBytes((int)ekeyHashSize);
        }
    }

    public class EKeySpecEntry
    {
        public byte[] EKey { get; private set; }
        public uint ESpecIndex { get; private set; }
        public ulong CompressedFileSize { get; private set; }

        public void Read(BinaryReader br, uint ekeyHashSize)
        {
            EKey = br.ReadBytes((int)ekeyHashSize);
            ESpecIndex = br.ReadUInt32().SwapEndian();
            CompressedFileSize = br.ReadUInt40(true);
        }
    }
}
