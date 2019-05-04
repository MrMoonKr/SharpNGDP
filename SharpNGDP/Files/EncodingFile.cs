using SharpNGDP.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpNGDP.Files
{
    public class EncodingFile : BLTEFile
    {
        public EncodingFile(Stream stream)
            : base(stream)
        { }

        public EncodingHeader EncodingHeader { get; private set; }
        public string[] ESpec { get; private set; }

        public EncodingTableIndex[] CEKeyTableIndices { get; private set; }
        public CEKeyPageTable[] CEKeyPageTable { get; private set; }

        public EncodingTableIndex[] ETableIndicies { get; private set; }
        public EKeySpecPageTable[] EKeySpecPageTable { get; private set; }


        public override void Read()
        {
            base.Read();

            using (var br = new BinaryReader(GetStream()))
            {
                EncodingHeader = new EncodingHeader();
                EncodingHeader.Read(br);

                var espec = new List<string>();
                var startPos = br.BaseStream.Position;
                while (br.BaseStream.Position < startPos + (long)EncodingHeader.ESpecBlockSize)
                    espec.Add(br.ReadCString());
                ESpec = espec.ToArray();

                CEKeyTableIndices = new EncodingTableIndex[EncodingHeader.CEKeyPageTablePageCount];
                for (var i = 0; i < CEKeyTableIndices.Length; i++)
                {
                    CEKeyTableIndices[i] = new EncodingTableIndex();
                    CEKeyTableIndices[i].Read(br, EncodingHeader.CKeyHashSize);
                }

                CEKeyPageTable = new CEKeyPageTable[EncodingHeader.CEKeyPageTablePageCount];
                for (var i = 0; i < CEKeyPageTable.Length; i++)
                {
                    var startRecord = br.BaseStream.Position;
                    CEKeyPageTable[i] = new CEKeyPageTable();
                    CEKeyPageTable[i].Read(br, EncodingHeader.CKeyHashSize, EncodingHeader.EKeyHashSize);
                    br.BaseStream.Position = startRecord + EncodingHeader.CEKeyPageTableSize * 1024;
                }

                ETableIndicies = new EncodingTableIndex[EncodingHeader.EKeySpecTablePageCount];
                for (var i = 0; i < ETableIndicies.Length; i++)
                {
                    ETableIndicies[i] = new EncodingTableIndex();
                    ETableIndicies[i].Read(br, EncodingHeader.EKeyHashSize);
                }

                EKeySpecPageTable = new EKeySpecPageTable[EncodingHeader.EKeySpecTablePageCount];
                for (var i = 0; i < EKeySpecPageTable.Length; i++)
                {
                    var startRecord = br.BaseStream.Position;
                    EKeySpecPageTable[i] = new EKeySpecPageTable();
                    EKeySpecPageTable[i].Read(br, EncodingHeader.EKeyHashSize);
                    br.BaseStream.Position = startRecord + EncodingHeader.EKeySpecPageTableSize * 1024;
                }
            }
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

            unk0 = br.ReadByte();

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

    public class CEKeyPageTable
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

    public class EKeySpecPageTable
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
