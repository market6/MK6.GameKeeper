using MK6.GameKeeper.AddIns.Contracts;
using MK6.GameKeeper.AddIns.HostView;
using System;
using System.AddIn.Pipeline;

namespace MK6.GameKeeper.AddIns.HostAdapter
{
    [HostAdapterAttribute]
    public class GameKeeperContractToViewHostSideAdapter : GameKeeperAddIn
    {
        private readonly GameKeeperAddInContract contract;
        public readonly ContractHandle ContractHandle;

        public GameKeeperContractToViewHostSideAdapter(GameKeeperAddInContract contract)
        {
            this.contract = contract;
            this.ContractHandle = new ContractHandle(contract);
        }

        public void Start()
        {
            contract.Start();
        }

        public void Stop()
        {
            contract.Stop();
        }


        public HostView.AddInStatus Status
        {
            get
            {
                var contractStatus = contract.Status;

                switch (contractStatus)
                {
                    case Contracts.AddInStatus.Running:
                        return HostView.AddInStatus.Running;
                    case Contracts.AddInStatus.Stopped:
                        return HostView.AddInStatus.Stopped;
                    default:
                        throw new Exception("Unknown status: " + contractStatus.ToString());
                }
            }
        }
    }
}
