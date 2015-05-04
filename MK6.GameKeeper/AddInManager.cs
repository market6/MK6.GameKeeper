using MK6.GameKeeper.AddIns.HostView;
using Serilog;
using System;
using System.AddIn.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MK6.GameKeeper
{
    class AddInManager
    {
        private static object installedAddinsLock = new object();

        private readonly DirectoryInfo pipelineDirectory;

        private readonly IDictionary<string, AddIn> installedAddins;

        private static AddInManager instance;

        private AddInManager(DirectoryInfo pipelineDirectory)
        {
            this.pipelineDirectory = pipelineDirectory;
            this.installedAddins = new Dictionary<string, AddIn>();

            AddInStore.Update(this.pipelineDirectory.FullName);
        }

        public static AddInManager Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new Exception("AddInManager has not been initialized");
                }

                return instance;
            }
        }

        public IEnumerable<AddIn> AddIns
        {
            get
            {
                return this.installedAddins.Values;
            }
        }

        public void Start()
        {
            EnsureHighestVersionAddinsAreInstalled();
            SetupPluginsDirectoryWatcher();
        }

        public void Stop()
        {
            lock (installedAddinsLock)
            {
                foreach (var addin in this.installedAddins.Values)
                {
                    addin.Dispose();
                }
            }
        }

        public static void Initialize(DirectoryInfo pipelineDirectory)
        {
            instance = new AddInManager(pipelineDirectory);
        }

        private void SetupPluginsDirectoryWatcher()
        {
            var watcher = new FileSystemWatcher(GetAddInsFolderPath());
            watcher.Created += OnAddInFolderAdded;
            watcher.Deleted += OnAddInFolderDeleted;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }

        private void OnAddInFolderAdded(object sender, FileSystemEventArgs e)
        {
            Log.Information("Plugin has been added");

            lock (installedAddinsLock)
            {
                EnsureHighestVersionAddinsAreInstalled();
            }
        }

        private void OnAddInFolderDeleted(object sender, FileSystemEventArgs e)
        {
            Log.Information("Plugin has been deleted");

            lock (installedAddinsLock)
            {
                RemoveUninstalledAddins();
            }
        }

        private void EnsureHighestVersionAddinsAreInstalled()
        {
            UpdateAddInStore();
            var addinTokensByName = GetAvailableAddIns().Select(t => new AddIn(t))
                .GroupBy(a => a.Name);

            foreach (var addinTokensForName in addinTokensByName)
            {
                var addinName = addinTokensForName.Key;
                Log.Verbose("Found {@Name} with versions {@Versions}", addinName, addinTokensForName.Select(t => t.Version));

                var highestVersionAddin = addinTokensForName.OrderByDescending(t => t.Version).First();
                var runningAddin = default(AddIn);

                if (installedAddins.TryGetValue(addinName, out runningAddin))
                {
                    Log.Verbose("Found running instance of addin {@Name} version {@Version}", runningAddin.Name, runningAddin.Version);

                    if (runningAddin.Version >= highestVersionAddin.Version)
                    {
                        Log.Debug("Currently running addin is same as latest");
                        continue;
                    }

                    Log.Debug("Stopping running addin {@Name} because a newer version is available", runningAddin.Name);
                    runningAddin.Dispose();
                    installedAddins.Remove(addinName);
                }

                Log.Debug("Starting addin {@Name} version {@Version}", highestVersionAddin.Name, highestVersionAddin.Version);
                installedAddins.Add(addinName, highestVersionAddin);
                highestVersionAddin.Start();
            }
        }

        private void RemoveUninstalledAddins()
        {
            var addinTokens = GetAvailableAddIns();

            foreach (var addinName in this.installedAddins.Keys.ToList())
            {
                if (!addinTokens.Any(t => t.Name == addinName))
                {
                    var addin = this.installedAddins[addinName];
                    addin.Stop();
                    this.installedAddins.Remove(addinName);
                }
            }
        }

        private void UpdateAddInStore()
        {
            var warnings = AddInStore.UpdateAddIns(GetAddInsFolderPath());

            foreach (var warning in warnings)
            {
                Log.Error(warning);
            }
        }

        private string GetAddInsFolderPath()
        {
            return Path.Combine(this.pipelineDirectory.FullName, "AddIns");
        }

        private IEnumerable<AddInToken> GetAvailableAddIns()
        {
            return AddInStore.FindAddIns(typeof(GameKeeperAddIn), this.pipelineDirectory.FullName);
        }
    }
}
