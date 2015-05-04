using MK6.GameKeeper.AddIns.HostView;
using Serilog;
using System;
using System.AddIn.Hosting;
using System.Diagnostics;

namespace MK6.GameKeeper
{
    class AddIn : IDisposable
    {
        public readonly string Id;
        public readonly string Name;
        public readonly Version Version;

        private AddInToken addintoken;
        private GameKeeperAddIn addin;
        private AddInProcess addinprocess;
        private Process systemprocess;

        public AddIn(AddInToken token)
        {
            addintoken = token;

            Name = token.Name;
            Version = new Version(token.Version);

            Id = token.Name.Replace(" ", "_");
        }

        public AddInStatus Status
        {
            get
            {
                if (addin == null)
                {
                    return AddInStatus.Stopped;
                }

                if (systemprocess.HasExited)
                {
                    return AddInStatus.Stopped;
                }

                var status = addin.Status;

                switch (status)
                {
                    case AddIns.HostView.AddInStatus.Running:
                        return AddInStatus.Running;
                    case AddIns.HostView.AddInStatus.Stopped:
                        return AddInStatus.Stopped;
                    default:
                        throw new Exception("unknown status: " + status.ToString());
                }
            }
        }

        public void Start()
        {
            if (addin == null)
            {
                try
                {
                    addinprocess = new AddInProcess();
                    Log.Verbose("Activating addin {@Name}", Name);
                    addin = addintoken.Activate<GameKeeperAddIn>(addinprocess, AddInSecurityLevel.FullTrust);
                    Log.Verbose("Adding {@Name} activated", this.Name);

                    systemprocess = Process.GetProcessById(addinprocess.ProcessId);
                    Log.Verbose(
                    "Addin {@Name} is activated in process id {@ProcessId}",
                    Name,
                    addinprocess.ProcessId);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while activating plugin {@Name}", Name);
                }
            }

            try
            {
                Log.Debug("Starting addin {@Name}", Name);
                this.addin.Start();
                Log.Debug("Addin {@Name} started", Name);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while starting plugin {@Name}", Name);
            }
        }

        public void Stop()
        {
            Log.Information("Shutting down addin {@Name}", Name);

            if (this.addin != null)
            {
                try
                {
                    if (Status != AddInStatus.Stopped)
                    {
                        Log.Debug("Stopping addin {@addinName}", Name);
                        addin.Stop();
                        Log.Debug("{@addinName} stopped ", Name);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while shutting down {@Name}", Name);
                }
            }
        }

        public void Dispose()
        {
            Stop();

            if (this.addinprocess != null)
            {
                try
                {
                    Log.Verbose("Shutting down process for addin {@Name}", Name);
                    addinprocess.Shutdown();
                    Log.Verbose("Process for addin {@Name} shutdown", Name);
                    addinprocess = null;

                    Log.Verbose(
                        "System process: {@ProcessId}, Exited: {@ProcessStatus}",
                        systemprocess.Id,
                        systemprocess.HasExited);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while shutting down process for {@Name}", Name);
                }
            }
        }
    }
}
