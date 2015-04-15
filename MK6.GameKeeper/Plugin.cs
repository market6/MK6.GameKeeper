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
            this.Thread.Start(this);
        }

        private static void RunPlugin(object pluginObj)
        {
            var plugin = pluginObj as Plugin;

            try
            {
                plugin.Domain.ExecuteAssembly(
                    plugin.Exe.FullName,
                    new[] { plugin.Id });
            }
            catch (ThreadAbortException)
            {
                // Do nothing; this is the thread shutting down
            }
            catch
            {
                System.Console.WriteLine("Caught within RunPlugin");

            }
        }

        public void Stop()
        {
            this.Thread.Abort();
            this.Thread.Join();
            AppDomain.Unload(this.Domain);
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
