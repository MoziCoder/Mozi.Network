using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 请求方法
    /// </summary>
    public class RequestMethod : AbsClassEnum
    {
        
        //HTTP/0.9
        /// <summary>
        /// GET方法请求一个指定资源的表示形式. 使用GET的请求应该只被用于获取数据.
        /// </summary>
        public static RequestMethod GET = new RequestMethod("GET");

        //HTTP/1.0
        /// <summary>
        /// HEAD方法请求一个与GET请求的响应相同的响应，但没有响应体.
        /// </summary>
        public static RequestMethod HEAD = new RequestMethod("HEAD");
        /// <summary>
        /// POST方法用于将实体提交到指定的资源，通常导致在服务器上的状态变化或副作用.
        /// </summary>
        public static RequestMethod POST = new RequestMethod("POST");

        //HTTP/1.1
        /// <summary>
        /// PUT方法用请求有效载荷替换目标资源的所有当前表示。
        /// </summary>
        public static RequestMethod PUT = new RequestMethod("PUT");
        /// <summary>
        /// DELETE方法删除指定的资源。
        /// </summary>
        public static RequestMethod DELETE = new RequestMethod("DELETE");
        /// <summary>
        /// CONNECT方法建立一个到由目标资源标识的服务器的隧道。
        /// </summary>
        public static RequestMethod CONNECT = new RequestMethod("CONNECT");
        /// <summary>
        /// OPTIONS方法用于描述目标资源的通信选项。
        /// </summary>
        public static RequestMethod OPTIONS = new RequestMethod("OPTIONS");
        /// <summary>
        /// TRACE方法沿着到目标资源的路径执行一个消息环回测试。
        /// </summary>
        public static RequestMethod TRACE = new RequestMethod("TRACE");
        /// <summary>
        /// PATCH方法用于对资源应用部分修改。
        /// </summary>
        public static RequestMethod PATCH = new RequestMethod("PATCH");

        //WEBDAV
        /// <summary>
        /// 从Web资源中检索以XML格式存储的属性。它也被重载，以允许一个检索远程系统的集合结构（也叫目录层次结构）
        /// </summary>
        public static RequestMethod PROPFIND = new RequestMethod("PROPFIND");
        /// <summary>
        /// 在单个原子性动作中更改和删除资源的多个属性
        /// </summary>
        public static RequestMethod PROPPATCH = new RequestMethod("PROPPATCH");
        /// <summary>
        /// 创建集合或者目录
        /// </summary>
        public static RequestMethod MKCOL = new RequestMethod("MKCOL");
        /// <summary>
        /// 将资源从一个URI复制到另外一个URI
        /// </summary>
        public static RequestMethod COPY = new RequestMethod("COPY");
        /// <summary>
        /// 将资源从一个URI移动到另外一个URI
        /// </summary>
        public static RequestMethod MOVE = new RequestMethod("MOVE");
        /// <summary>
        /// 锁定一个资源。WebDAV支持共享锁和互斥锁
        /// </summary>
        public static RequestMethod LOCK = new RequestMethod("LOCK");
        /// <summary>
        /// 解除资源的锁定
        /// </summary>
        public static RequestMethod UNLOCK = new RequestMethod("UNLOCK");

        /// <summary>
        /// 方法名
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        protected override string Tag { get { return Name; } }

        private string _name;

        public RequestMethod(string name)
        {
            _name = name;
        }

    }
}
