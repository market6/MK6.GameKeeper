using MK6.GameKeeper.AddIns;
using Contract = MK6.GameKeeper.AddIns.Contracts;
using System;
using System.AddIn.Pipeline;

namespace MK6.GameKeeper.AddIns.AddInAdapter
{
    [AddInAdapterAttribute]
    public class GameKeeperViewToContractAddInSideAdapter : ContractBase, Contract.GameKeeperAddInContract
    {
        private readonly GameKeeperAddIn addin;

        public GameKeeperViewToContractAddInSideAdapter(GameKeeperAddIn addin)
        {
            this.addin = addin;
        }

        public void Start()
        {
            addin.Start();
        }

        public void Stop()
        {
            addin.Stop();
        }

        public Contract.AddInStatus Status
        {
            get
            {
                var addinstatus = addin.Status;

                switch (addinstatus)
                {
                    case AddInStatus.Running:
                        return Contract.AddInStatus.Running;
                    case AddInStatus.Stopped:
                        return Contract.AddInStatus.Stopped;
                    default:
                        throw new Exception("Unknown status: " + addinstatus.ToString());
                }
            }
        }
    }
}
