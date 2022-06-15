using Mozi.TLS.Common;

namespace Mozi.TLS
{
    /// <summary>
    /// 传输加密类型
    /// </summary>
    public class TLSVersion : AbsClassEnum
    {
        private readonly string _tag = "";
        private readonly ushort _code = 0;

        /// <summary>
        /// 唯一标识符号
        /// </summary>
        protected override string Tag { get { return _tag; } }
        /// <summary>
        /// 版本代码
        /// </summary>
        public ushort Code { get { return _code; } }
        /// <summary>
        /// SSL3.0
        /// </summary>
        public static readonly TLSVersion SSL30 = new TLSVersion("SSL30", 0);
        /// <summary>
        /// TLS1.0
        /// </summary>
        public static readonly TLSVersion TLS10 = new TLSVersion("TLS10", 0x0103);
        /// <summary>
        /// TLS1.1
        /// </summary>
        public static readonly TLSVersion TLS11 = new TLSVersion("TLS11", 0);
        /// <summary>
        /// TLS1.2
        /// </summary>
        public static readonly TLSVersion TLS12 = new TLSVersion("TLS12", 0);
        /// <summary>
        /// TLS1.3
        /// </summary>
        public static readonly TLSVersion TLS13 = new TLSVersion("TLS13", 0);
        /// <summary>
        /// TLS
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="code"></param>
        public TLSVersion(string tag, ushort code)
        {
            _tag = tag;
            _code = code;
        }
    }
}
