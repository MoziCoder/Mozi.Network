using Mozi.HttpEmbedded.Page;

namespace Mozi.HttpEmbedded.Test
{
    public class Test:BaseApi
    {
        public string Guid()
        {
            return SSDP.UUID.Generate();
        }
    }
}
