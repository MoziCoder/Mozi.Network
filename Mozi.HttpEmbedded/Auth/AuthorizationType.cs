using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded.Auth
{
    /// <summary>
    /// HTTP��֤����
    /// </summary>
    public class AuthorizationType : AbsClassEnum
    {
        /// <summary>
        /// ������֤ ���Ĵ��� ����ȫ
        /// </summary>
        public static readonly AuthorizationType Basic = new AuthorizationType("Basic");

        /// <summary>
        /// DigestժҪ��֤
        /// </summary>
        public static AuthorizationType Digest = new AuthorizationType("Digest");

        internal static AuthorizationType WSSE = new AuthorizationType("WSSE");

        //Bearer
        //HOBA
        //Mutual

        /// <summary>
        /// ����֤
        /// </summary>
        public static AuthorizationType None = new AuthorizationType("none");

        private string _name;
        /// <summary>
        /// ��֤���͵����ƣ����ִ�Сд
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        /// <summary>
        /// Ψһ��ʶ����
        /// </summary>
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