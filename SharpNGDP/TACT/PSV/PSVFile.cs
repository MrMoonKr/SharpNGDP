using System.Collections.Generic;

namespace SharpNGDP.TACT.PSV
{
    public class PSVFile
    {
        public PSVFile(string seqn, string[] header, string[][] rows)
        {
            SequenceNumber = seqn;
            Header = header;
            Rows = rows;
        }

        public string SequenceNumber { get; }

        public string[] Header { get; }
        public string[][] Rows { get; }

        //public string GetRowColumn(uint row, string key) => GetRowColumn(Rows[row], key);
        //public string GetRowColumn(string[] row, string key) => row[Array.IndexOf(Header, key)];

        // TODO: *NOT* ideal
        public IEnumerable<T> MapTo<T>() where T : PSVResponse, new()
        {
            for (var r = 0; r < Rows.Length; r++)
            {
                var mappedRow = new T();
                for (var c = 0; c < Header.Length; c++)
                    mappedRow.Map(Header[c], Rows[r][c]);
                yield return mappedRow;
            }
        }
    }
}
