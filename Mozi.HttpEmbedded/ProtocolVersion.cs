using System;
using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// HTTP协议版本,其它类HTTP协议也可以照此进行封装
    /// </summary>
    public class ProtocolVersion : AbsClassEnum
    {
        /// <summary>
        /// HTTP/0.9 仅支持GET方法 响应只支持HTML内容
        /// </summary>
        [Obsolete("实现HTTP/0.9没有意义")]
        public static readonly ProtocolVersion Version09 = new ProtocolVersion("HTTP", "0.9");
        /// <summary>
        /// HTTP/1.0
        /// </summary>
        public static readonly ProtocolVersion Version10 = new ProtocolVersion("HTTP", "1.0");
        /// <summary>
        /// HTTP/2.0
        /// </summary>
        public static readonly ProtocolVersion Version11 = new ProtocolVersion("HTTP", "1.1");

        /// <summary>
        /// 1. 二进制协议
        ///     HTTP/1.1 版的头信息肯定是文本（ASCII编码），数据体可以是文本，也可以是二进制。HTTP/2 则是一个彻底的二进制协议，头信息和数据体都是二进制，并且统称为”帧”：头信息帧和数据帧。
        ///     二进制协议解析起来更高效、“线上”更紧凑，更重要的是错误更少。
        /// 2. 完全多路复用
        ///     HTTP/2 复用TCP连接，在一个连接里，客户端和浏览器都可以同时发送多个请求或回应，而且不用按照顺序一一对应，这样就避免了”队头堵塞”。
        /// 3. 报头压缩
        ///     HTTP 协议是没有状态，导致每次请求都必须附上所有信息。所以，请求的很多头字段都是重复的，比如Cookie，一样的内容每次请求都必须附带，这会浪费很多带宽，也影响速度。
        ///     对于相同的头部，不必再通过请求发送，只需发送一次；
        ///     HTTP/2 对这一点做了优化，引入了头信息压缩机制；
        ///     一方面，头信息使用gzip或compress压缩后再发送；
        ///     另一方面，客户端和服务器同时维护一张头信息表，所有字段都会存入这个表，产生一个索引号，之后就不发送同样字段了，只需发送索引号。
        /// 4. 服务器推送
        ///     HTTP/2 允许服务器未经请求，主动向客户端发送资源；
        ///     通过推送那些服务器任务客户端将会需要的内容到客户端的缓存中，避免往返的延迟
        /// </summary>
        public static readonly ProtocolVersion Version20 = new ProtocolVersion("HTTP", "2.0");
        /// <summary>
        /// Http3.0在Http2.0的基础上使用QUIC控制协议，UDP协议
        /// </summary>
        public static readonly ProtocolVersion Version30 = new ProtocolVersion("HTTP", "3.0");
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get { return _vervalue; } }
        /// <summary>
        /// 协议类型
        /// </summary>
        public string Name { get { return _protoName; } }
        /// <summary>
        /// 唯一标识符
        /// </summary>
        protected override string Tag { get { return _protoName.ToUpper()+"/"+_vervalue; } }

        private string _vervalue = "", _protoName = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="protoName"></param>
        /// <param name="vervalue"></param>
        public ProtocolVersion(string protoName, string vervalue)
        {
            _vervalue = vervalue;
            _protoName = protoName;
        }
        /// <summary>
        /// 重写ToString方法
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{_protoName}/{_vervalue}";
        }
    }
}