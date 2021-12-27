using System.IO;


namespace SharpNGDP.TACT
{
    public abstract class TACTFile : NGDPFile
    {
        public TACTFile( Stream stream )
            : base( stream )
        {
            // nothing
        }
    }
}
