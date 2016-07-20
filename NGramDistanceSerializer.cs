using Augury.Base;
using System;
using System.IO;

namespace Augury.Lucene
{
    public class NGramDistanceSerializer : ISerializer<NGramDistance>
    {
        public NGramDistance Deserialize(Stream stream)
        {
            var bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            var n = BitConverter.ToInt32(bytes, 0);
            return new NGramDistance(n);
        }

        public void Serialize(Stream stream, NGramDistance data)
        {
            stream.Write(BitConverter.GetBytes(data.N), 0, 4);
        }
    }
}
