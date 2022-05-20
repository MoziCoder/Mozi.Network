namespace Mozi.SSDP
{
    public interface ISSDPService
    {
        void Activate();
        void Inactivate();
        void Search(SearchPackage pk);
        void Search(TargetDesc desc);
        void StartAdvertise();
        void StopAdvertise();
    }
}
