using System.Collections.Generic;

namespace Mozi.HttpEmbedded.Serialize
{
    /// <summary>
    /// 数据序列化接口
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// 内容格式
        /// </summary>
        string ContentType { get; }
        /// <summary>
        /// 序列化类型
        /// </summary>
        DataSerializeType SerialzeType { get; }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        string Encode(object data);
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        T Decode<T>(string data);
        /// <summary>
        /// 集合反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        IEnumerable<T> DecodeList<T>(string data);
    }
}
