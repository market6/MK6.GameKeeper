namespace MK6.GameKeeper.AddIns.HostView
{
    public interface GameKeeperAddIn
    {
        void Start();
        void Stop();
        AddInStatus Status { get; }
    }
}
