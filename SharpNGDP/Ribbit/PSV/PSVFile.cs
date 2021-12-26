﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace SharpNGDP.Ribbit.PSV
{
    public class PSVFile : RibbitFile
    {
        private static Logger log = Logger.Create<PSVFile>();

        private static Regex s_SequenceNumberRegex = new Regex(@"seqn = (\d+)", RegexOptions.Compiled);

        public PSVFile( Stream stream )
            : base( stream )
        { }

        public string SequenceNumber { get; private set; }

        public string[] Header { get; private set; }
        public string[][] Rows { get; private set; }

        public IEnumerable<T> AsRecords<T>() where T : PSVRecord, new()
        {
            foreach ( var row in Rows )
            {
                var record = new T();
                record.Read( Header , row );
                yield return record;
            }
        }

        public override void Read()
        {
            base.Read();

            var sw = Stopwatch.StartNew();

            using ( var sr = new StreamReader( GetStream() ) )
            {
                if ( sr.EndOfStream )
                    throw new PSVParseException( "No input" );

                // Read header
                var header = sr.ReadLine().Split('|');
                for ( var c = 0; c < header.Length; c++ )
                    header[ c ] = header[ c ].Substring( 0 , header[ c ].IndexOf( '!' ) );

                var seqn = string.Empty;

                var rows = new List<string[]>();
                while ( sr.Peek() > 0 )
                {
                    var line = sr.ReadLine();

                    // Ignore empty lines
                    if ( string.IsNullOrEmpty( line ) )
                        continue;

                    // Ignore comments
                    if ( line.StartsWith( "#" ) )
                    {
                        // ... unless it's the sequence number :)
                        var seqnMatch = s_SequenceNumberRegex.Match(line);
                        if ( seqnMatch.Success )
                            seqn = seqnMatch.Groups[ 1 ].Value;

                        continue;
                    }

                    var row = line.Split('|');
                    if ( header.Length != row.Length )
                        throw new PSVParseException( $"Column number mismatch between header ({header.Length}) and row ({row.Length}) at row {line}" );
                    rows.Add( row );
                }

                SequenceNumber = seqn;
                Header = header;
                Rows = rows.ToArray();
            }

            sw.Stop();
            log.WriteLine( $"Parsed PSVFile in {sw.Elapsed}" );
            log.WriteLine( $"Sequence Number: {SequenceNumber}" );
            log.WriteLine( $"{Rows.Length} rows with {Header.Length} columns" );
        }
    }

    public class PSVParseException : Exception
    {
        public PSVParseException()
        {
        }

        public PSVParseException( string message )
            : base( message )
        {
        }
    }
}
