using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Topshelf;

namespace MK6.GameKeeper
{
    class Service : ServiceControl
    {
        private static object _pluginListLock = new object();

        private readonly DirectoryInfo pluginsDirectory;

        private readonly int watchdogIntervalMilliseconds;

        private readonly List<Plugin> plugins;

        public Service(DirectoryInfo pluginsDirectory, int watchdogIntervalMilliseconds)
        {
            this.pluginsDirectory = pluginsDirectory;
            this.watchdogIntervalMilliseconds = watchdogIntervalMilliseconds;
            this.plugins = new List<Plugin>();

            if (!this.pluginsDirectory.Exists)
            {
                this.pluginsDirectory.Create();
            }
        }

        public bool Start(HostControl hostControl)
        {
            StartPluginsNotCurrentlyRunning();
            SetupPluginsDirectoryWatcher(pluginsDirectory, plugins);

            var watchdogTimer = new Timer(
                CleanupCrashedPlugins, 
                null, 
                0, 
                watchdogIntervalMilliseconds);

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            lock (_pluginListLock)
            {
                for (var pluginIndex = 0; pluginIndex < plugins.Count; pluginIndex += 1)
                {
                    plugins[pluginIndex].Stop();
                }
            }

            return true;
        }

        private void SetupPluginsDirectoryWatcher(DirectoryInfo pluginsDirectory, List<Plugin> plugins)
        {
            var watcher = new FileSystemWatcher(pluginsDirectory.FullName);
            watcher.Created += OnPluginDirectoryAdded(pluginsDirectory, plugins);
            watcher.Changed += OnPluginDirectoryChanged(pluginsDirectory, plugins);
            watcher.Deleted += OnPluginDirectoryDeleted(plugins);
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }

        private FileSystemEventHandler OnPluginDirectoryAdded(
            DirectoryInfo pluginsDirectory,
            List<Plugin> plugins)
        {
            return (sender, e) =>
            {
                Log.Information("Plugin has been added");

                lock (_pluginListLock)
                {
                    StartPluginsNotCurrentlyRunning();
                }
            };
        }

        private FileSystemEventHandler OnPluginDirectoryChanged(
            DirectoryInfo pluginsDirectory,
            List<Plugin> plugins)
        {
            return (sender, e) =>
            {
                Log.Information("Plugin directory {@AffectedDirectory} has been changed", e.FullPath);

                lock (_pluginListLock)
                {
                    var affectedDirectory = Directory.Exists(e.FullPath)
                        ? new DirectoryInfo(e.FullPath)
                        : new FileInfo(e.FullPath).Directory;

                    var affectedPlugins = plugins
                        .Where(p => p.Directory.FullName == affectedDirectory.FullName)
                        .ToList();

                    Log.Debug("Plugins affected by changed {@AffectedPlugins}", affectedPlugins.Select(p => p.Id));

                    StopPlugins(affectedPlugins);
                    RemovePluginsThatNoLongerExist();
                    StartPluginsNotCurrentlyRunning();
                }
            };
        }

        private FileSystemEventHandler OnPluginDirectoryDeleted(List<Plugin> plugins)
        {
            return (sender, e) =>
            {
                Log.Information("Plugin has been deleted");

                lock (_pluginListLock)
                {
                    RemovePluginsThatNoLongerExist();
                }
            };
        }

        private void StartPluginsNotCurrentlyRunning()
        {
            var pluginFoldersNotRunning = pluginsDirectory.EnumerateDirectories()
                .Where(pluginFolder => !plugins.Any(plugin => plugin.Directory.FullName == pluginFolder.FullName));

            foreach (var pluginFolderToAdd in pluginFoldersNotRunning)
            {
                var pluginExe = pluginFolderToAdd.EnumerateFiles("*.exe").FirstOrDefault();

                if (pluginExe == null)
                {
                    continue;
                }

                Log.Information("Starting plugin {@PluginName}", pluginFolderToAdd.Name);
                plugins.Add(StartPlugin(pluginFolderToAdd));
            }
        }

        private void RemovePluginsThatNoLongerExist()
        {
            var pluginsToRemove = plugins.Where(plugin => !Directory.Exists(plugin.Directory.FullName)).ToList();
            StopPlugins(pluginsToRemove);
        }

        private void CleanupCrashedPlugins(object ignoreMe)
        {
            Log.Debug("Currently running plugins {@PluginNames}", this.plugins.Select(p => p.Id));

            for (var pluginIndex = plugins.Count - 1; pluginIndex >= 0; pluginIndex -= 1)
            {
                var plugin = plugins[pluginIndex];

                if (plugin.Thread.ThreadState != ThreadState.Stopped)
                {
                    Log.Verbose("Watchdog found that plugin {@PluginName} is still running", plugin.Id);
                    continue;
                }

                Log.Error("Watchdog found that the thread for plugin {@PluginName} has stopped", plugin.Id);
                plugin.Stop();

                plugins.RemoveAt(pluginIndex);
            }

            StartPluginsNotCurrentlyRunning();
        }

        private void StopPlugins(IEnumerable<Plugin> pluginsToRemove)
        {
            foreach (var pluginToRemove in pluginsToRemove)
            {
                Log.Information("Stopping plugin {@PluginName}", pluginToRemove.Id);
                pluginToRemove.Stop();
                plugins.Remove(pluginToRemove);
            }
        }

        private Plugin StartPlugin(DirectoryInfo pluginFolder)
        {
            var plugin = new Plugin(pluginFolder);

            plugin.Start();

            return plugin;
        }
    }
}
