using System.AddIn.Contract;
using System.AddIn.Pipeline;

namespace MK6.GameKeeper.AddIns.Contracts
{
    [AddInContract]
    public interface GameKeeperAddInContract : IContract
    {
        void Start();
        void Stop();
        AddInStatus Status { get; }
    }
}
