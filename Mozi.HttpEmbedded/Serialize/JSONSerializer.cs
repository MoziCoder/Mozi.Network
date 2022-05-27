using System;
using System.Collections.Generic;

namespace Mozi.HttpEmbedded.Serialize
{
    /// <summary>
    /// JSON序列化
    /// </summary>
    internal class JSONSerializer : ISerializer
    {
        public DataSerializeType SerialzeType => throw new NotImplementedException();

        public string ContentType => "application/json";

        public T Decode<T>(string data)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> DecodeList<T>(string data)
        {
            throw new NotImplementedException();
        }

        public string Encode(object data)
        {
            throw new NotImplementedException();
        }
    }
}
