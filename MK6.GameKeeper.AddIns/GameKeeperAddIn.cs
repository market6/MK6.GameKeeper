using System.AddIn.Pipeline;

namespace MK6.GameKeeper.AddIns
{
    [AddInBase]
    public interface GameKeeperAddIn
    {
        void Start();
        void Stop();
        AddInStatus Status { get; }
    }
}
