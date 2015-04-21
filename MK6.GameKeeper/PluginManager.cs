using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace MK6.GameKeeper
{
    class PluginManager
    {
        private static object _pluginListLock = new object();
        private static Lazy<PluginManager> _instance;
        private readonly DirectoryInfo _pluginsDirectory;
        private readonly List<Plugin> _plugins = new List<Plugin>(); 

        static PluginManager()
        {
            var pluginDirectoryName = ConfigurationManager.AppSettings["PluginDirectory"];
            var pluginsDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, pluginDirectoryName));
            if (pluginsDirectory.Exists)
            {
                pluginsDirectory.Create();
            }

            _instance = new Lazy<PluginManager>(() => new PluginManager(pluginsDirectory));
        }

        private PluginManager(DirectoryInfo pluginsDirectory)
        {
            _pluginsDirectory = pluginsDirectory;
        }

        internal static PluginManager Instance
        {
            get { return _instance.Value; }
        }

        internal List<Plugin> Plugins { get { return _plugins; } }

        internal DirectoryInfo PluginsDirectory { get { return _pluginsDirectory; } }

        internal void StopPlugins()
        {
            lock (_pluginListLock)
            {
                for (var pluginIndex = 0; pluginIndex < _plugins.Count; pluginIndex += 1)
                {
                    _plugins[pluginIndex].Stop();
                }
            }
        }

        internal void StartPluginsNotCurrentlyRunning()
        {
            lock (_pluginListLock)
            {
                var pluginFoldersNotRunning = _pluginsDirectory.EnumerateDirectories()
                    .Where(pluginFolder => !_plugins.Any(plugin => plugin.Directory.FullName == pluginFolder.FullName));

                foreach (var pluginFolderToAdd in pluginFoldersNotRunning)
                {
                    var pluginExe = pluginFolderToAdd.EnumerateFiles("*.exe").FirstOrDefault();

                    if (pluginExe == null)
                    {
                        continue;
                    }

                    Log.Information("Starting plugin {@PluginName}", pluginFolderToAdd.Name);
                    _plugins.Add(StartPlugin(pluginFolderToAdd));
                }
            }
        }

        internal void RemovePluginsThatNoLongerExist()
        {
            lock (_pluginListLock)
            {
                var pluginsToRemove = _plugins.Where(plugin => !Directory.Exists(plugin.Directory.FullName)).ToList();
                StopPlugins(pluginsToRemove);
            }
        }

        internal void CleanupCrashedPlugins(object ignoreMe)
        {
            Log.Debug("Currently running plugins {@PluginNames}", this._plugins.Select(p => p.Id));

            for (var pluginIndex = _plugins.Count - 1; pluginIndex >= 0; pluginIndex -= 1)
            {
                var plugin = _plugins[pluginIndex];

                if (plugin.Thread.ThreadState != ThreadState.Stopped)
                {
                    Log.Verbose("Watchdog found that plugin {@PluginName} is still running", plugin.Id);
                    continue;
                }

                Log.Error("Watchdog found that the thread for plugin {@PluginName} has stopped", plugin.Id);
                plugin.Stop();

                _plugins.RemoveAt(pluginIndex);
            }

            StartPluginsNotCurrentlyRunning();
        }

        internal void StopPlugins(IEnumerable<Plugin> pluginsToRemove)
        {
            foreach (var pluginToRemove in pluginsToRemove)
            {
                Log.Information("Stopping plugin {@PluginName}", pluginToRemove.Id);
                pluginToRemove.Stop();
                _plugins.Remove(pluginToRemove);
            }
        }

        internal void PluginDirectoryChange(FileSystemEventArgs e)
        {
            lock (_pluginListLock)
            {
                var affectedDirectory = Directory.Exists(e.FullPath)
                    ? new DirectoryInfo(e.FullPath)
                    : new FileInfo(e.FullPath).Directory;

                var affectedPlugins = _plugins
                    .Where(p => p.Directory.FullName == affectedDirectory.FullName)
                    .ToList();

                Log.Debug("Plugins affected by changed {@AffectedPlugins}", affectedPlugins.Select(p => p.Id));

                StopPlugins(affectedPlugins);
                RemovePluginsThatNoLongerExist();
                StartPluginsNotCurrentlyRunning();
            }
        }

        private static Plugin StartPlugin(DirectoryInfo pluginFolder)
        {
            var plugin = new Plugin(pluginFolder);

            plugin.Start();

            return plugin;
        }
    }
}
