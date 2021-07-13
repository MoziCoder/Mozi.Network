namespace Mozi.HttpEmbedded.Middleaware
{
    /// <summary>
    /// 中间件 接入多层次逻辑处理
    /// </summary>
    internal interface IMiddleware
    {
        void Invoke();
    }
}
