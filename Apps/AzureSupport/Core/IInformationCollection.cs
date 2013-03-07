namespace TheBall.CORE
{
    public interface IInformationCollection
    {
        string GetItemDirectory();
        void RefreshContent();
        void SubscribeToContentSource();
        bool IsMasterCollection { get; }
        string GetMasterLocation();
        IInformationCollection GetMasterInstance();
    }
}