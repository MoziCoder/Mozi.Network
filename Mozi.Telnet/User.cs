namespace Mozi.Telnet
{
    /// <summary>
    /// 服务器用户
    /// </summary>
    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
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
