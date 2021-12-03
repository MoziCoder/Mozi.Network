namespace Mozi.TLS
{
    /// <summary>
    /// 传输加密类型
    /// </summary>
    public class TLSVersion : AbsClassEnum
    {
        private readonly string _tag = "";
        private readonly ushort _code = 0;

        protected override string Tag { get { return _tag; } }
        public ushort Code { get { return _code; } }

        public static readonly TLSVersion SSL30 = new TLSVersion("SSL30", 0);
        public static readonly TLSVersion TLS10 = new TLSVersion("TLS10", 0x0103);
        public static readonly TLSVersion TLS11 = new TLSVersion("TLS11", 0);
        public static readonly TLSVersion TLS12 = new TLSVersion("TLS12", 0);
        public static readonly TLSVersion TLS13 = new TLSVersion("TLS13", 0);

        public TLSVersion(string tag, ushort code)
        {
            _tag = tag;
            _code = code;
        }
    }
}
