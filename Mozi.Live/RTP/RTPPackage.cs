namespace Mozi.Live.RTP
{
    /// <summary>
    /// RTP数据包封装
    /// </summary>
    public class RTPPackage
    {

    }
    /// <summary>
    /// 
    /// </summary>
    public abstract class RTSPResource
    {
        HttpEmbedded.HttpContext Context;

        public virtual void Describe()
        {
            
        }
        public virtual void Play()
        {
            
        }

        public virtual void Pause()
        {
            
        }

        public virtual void Setup()
        {
            
        }

        public virtual void Teardown()
        {
            
        }

        public virtual void SET_PARAMETER()
        {
           
        }

        public virtual void GET_PARAMETER()
        {
            
        }
    }
}
