using System.IO;

namespace SharpNGDP
{
    /// <summary>
    /// 응답 컨텐츠 파일 추상화
    /// </summary>
    public abstract class NGDPFile
    {
        public NGDPFile( Stream stream )
        {
            BaseStream = stream;
        }

        public Stream BaseStream { get; }

        /// <summary>
        /// 데이터 스트림 반환.
        /// </summary>
        /// <returns></returns>
        public virtual Stream GetStream() => BaseStream;

        public abstract void Read();
    }
}
