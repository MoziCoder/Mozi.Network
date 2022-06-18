namespace Mozi.HttpEmbedded.Auth
{
    /// <summary>
    /// 服务器用户
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 用户分组
        /// </summary>
        public UserGroup UserGroup { get; set; }
    }
    /// <summary>
    /// 用户组
    /// </summary>
    public enum UserGroup
    {
        User = 0,
        Admin = 1 //管理组
    }
}
