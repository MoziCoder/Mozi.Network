using System;

namespace Mozi.StateService
{
    /// <summary>
    /// 心跳订阅者 订阅者为已知订阅者
    /// </summary>
    public class HeartBeatSubscriber : HeartBeatGateway
    {
        /// <summary>
        /// 终端消息接收事件
        /// </summary>
        public new  ClientMessageReceived OnClientMessageReceived;
        /// <summary>
        /// 数据接收完成回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void Socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            try
            {
                GC.Collect();
                HeartBeatPublishPackage pg = HeartBeatPublishPackage.Parse(args.Data);
                HeartBeatPackage hbp = pg.HeartBeat;
                ClientAliveInfo ca = new ClientAliveInfo()
                {
                    DeviceName = hbp.DeviceName,
                    DeviceId = hbp.DeviceId,
                    AppVersion = hbp.AppVersion,
                    UserName = hbp.UserName,
                    State = (ClientLifeState)Enum.Parse(typeof(ClientLifeState), hbp.StateName.ToString()),
                    Host=pg.SrcHost
                };
                var client = UpsertClient(ca);
                if (OnClientMessageReceived != null)
                {
                    OnClientMessageReceived(this, client, pg.SrcHost, pg.SrcPort);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
