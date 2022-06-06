namespace Mozi.SSDP
{
    /// <summary>
    /// SSDPService接口
    /// </summary>
    public interface ISSDPService
    {
        /// <summary>
        /// 激活服务
        /// </summary>
        void Activate();
        /// <summary>
        /// 反激活服务
        /// </summary>
        void Inactivate();
        /// <summary>
        /// 搜索对象
        /// </summary>
        /// <param name="pk"></param>
        void Search(SearchPackage pk);
        /// <summary>
        /// 搜索对象
        /// </summary>
        /// <param name="desc"></param>
        void Search(TargetDesc desc);
        /// <summary>
        /// 启用定时发送在线公告
        /// </summary>
        void StartAdvertise();
        /// <summary>
        /// 停止定时发送在线公告
        /// </summary>
        void StopAdvertise();
    }
}
