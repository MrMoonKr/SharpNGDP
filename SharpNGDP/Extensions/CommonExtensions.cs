using System.Linq;

namespace SharpNGDP.Extensions
{
    public static class CommonExtensions
    {
        public static string ToHexString( this byte[] bytes ) =>
            string.Join( "" , bytes.Select( b => b.ToString( "x2" ) ) );
    }
}
