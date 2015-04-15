using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace MK6.GameKeeper
{
    class Plugin : IDisposable
    {
        public readonly string Id;

        public readonly DirectoryInfo Directory;

        public readonly FileInfo Exe;

        public readonly Thread Thread;

        public readonly AppDomain Domain;

        public Plugin(DirectoryInfo pluginFolder)
        {
            this.Id = pluginFolder.Name;
            this.Directory = pluginFolder;

            this.Exe = pluginFolder.EnumerateFiles("*.exe").SingleOrDefault();
            var appDomainSetup = new AppDomainSetup
            {
                ShadowCopyFiles = "true",
                ConfigurationFile = this.Exe.FullName + ".config"
            };

            this.Domain = AppDomain.CreateDomain(this.Directory.Name, null, appDomainSetup);
            this.Thread = new Thread(new ParameterizedThreadStart(RunPlugin));
        }

        public void Start()
        {
            Log.Debug("Starting plugin {@PluginName} thread", this.Id);
            this.Thread.Start(this);
            Log.Debug("Started plugin {@PluginName} thread", this.Id);
        }

        private static void RunPlugin(object pluginObj)
        {
            var plugin = pluginObj as Plugin;

            try
            {
                Log.Debug(
                    "Executing {@Exe} in application domain {@AppDomain}", 
                    plugin.Exe.FullName, 
                    plugin.Domain.FriendlyName);
                plugin.Domain.ExecuteAssembly(
                    plugin.Exe.FullName,
                    new[] { plugin.Id });
            }
            catch (ThreadAbortException)
            {
                // Do nothing; this is the thread shutting down
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error from plugin {@PluginName} bubbled up to GameKeeper", plugin.Id);

            }
        }

        public void Stop()
        {
            Log.Debug("Stopping plugin {@PluginName} thread", this.Id);
            Log.Debug("Aborting thread for plugin {@PluginName}", this.Id);
            this.Thread.Abort();
            Log.Debug("Waiting for thread for plugin {@PluginName} to join", this.Id);
            this.Thread.Join();

            Log.Debug("Unloading app domain for plugin {@PluginName}", this.Id);
            AppDomain.Unload(this.Domain);
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
