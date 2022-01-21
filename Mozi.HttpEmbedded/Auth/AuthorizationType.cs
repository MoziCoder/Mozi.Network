using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded.Auth
{
    /// <summary>
    /// HTTP认证类型
    /// </summary>
    public class AuthorizationType : AbsClassEnum
    {
        public const string REALM/*提示明文*/ = "HttpEmbedded";

        /// <summary>
        /// 基本认证 明文传输 不安全
        /// </summary>
        public static readonly AuthorizationType Basic = new AuthorizationType("Basic");

        //TODO 未实现高级认证
        internal static AuthorizationType Digest = new AuthorizationType("Digest");
        internal static AuthorizationType WSSE = new AuthorizationType("WSSE");

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        protected override string Tag
        {
            get { return _name; }
        }

        private AuthorizationType(string name)
        {
            _name = name;
        }
    }
}