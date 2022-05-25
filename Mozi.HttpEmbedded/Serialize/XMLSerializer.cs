using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Mozi.HttpEmbedded.Serialize
{
    /// <summary>
    /// XML序列化
    /// </summary>
    public class XMLSerializer : ISerializer
    {
        public string ContentType => "text/xml";

        public DataSerializeType SerialzeType => DataSerializeType.XML;

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public T Decode<T>(string data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(data))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
        /// <summary>
        /// 反列表序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public IEnumerable<T> DecodeList<T>(string data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
            using (StringReader reader = new StringReader(data))
            {
                return (List<T>)serializer.Deserialize(reader);
            }
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Encode(object data)
        {
            XmlSerializer serializer = new XmlSerializer(data.GetType());
            string content = string.Empty;
            //serialize
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, data);
                writer.Flush();
                content = writer.ToString();
                writer.Dispose();
            };
            return content;
        }
    }
}
