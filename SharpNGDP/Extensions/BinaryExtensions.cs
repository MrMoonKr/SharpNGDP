using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpNGDP.Extensions
{
    public static class BinaryExtensions
    {
        public static ulong ReadUInt40( this BinaryReader br , bool bigEndian = false )
        {
            var fsb = br.ReadBytes(5);
            if ( bigEndian )
                fsb = fsb.ReverseBytes();
            return ( ulong )( fsb[ 4 ] << 32 | fsb[ 3 ] << 24 | fsb[ 2 ] << 16 | ( fsb[ 1 ] << 8 ) | fsb[ 0 ] );
        }

        public static long Remaining( this BinaryReader br ) =>
            br.BaseStream.Length - br.BaseStream.Position;

        public static byte[] ReverseBytes( this byte[] bytes )
        {
            var newBytes = new byte[bytes.Length];
            Array.Copy( bytes , newBytes , bytes.Length );
            Array.Reverse( newBytes );
            return newBytes;
        }

        public static short SwapEndian( this short obj ) =>
            BitConverter.ToInt16( BitConverter.GetBytes( obj ).ReverseBytes() , 0 );
        public static int SwapEndian( this int obj ) =>
            BitConverter.ToInt32( BitConverter.GetBytes( obj ).ReverseBytes() , 0 );
        public static long SwapEndian( this long obj ) =>
            BitConverter.ToInt64( BitConverter.GetBytes( obj ).ReverseBytes() , 0 );


        public static ushort SwapEndian( this ushort obj ) =>
            BitConverter.ToUInt16( BitConverter.GetBytes( obj ).ReverseBytes() , 0 );
        public static uint SwapEndian( this uint obj ) =>
            BitConverter.ToUInt32( BitConverter.GetBytes( obj ).ReverseBytes() , 0 );
        public static ulong SwapEndian( this ulong obj ) =>
            BitConverter.ToUInt64( BitConverter.GetBytes( obj ).ReverseBytes() , 0 );

        public static string ReadCString( this BinaryReader br ) =>
            ReadCString( br , Encoding.UTF8 );

        public static string ReadCString( this BinaryReader br , Encoding encoding )
        {
            var buffer = new List<byte>();
            byte b = 0;
            while ( ( b = br.ReadByte() ) != 0 )
                buffer.Add( b );

            if ( buffer.Count <= 0 )
                return null;

            return encoding.GetString( buffer.ToArray() );
        }

    }
}
