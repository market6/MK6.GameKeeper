using System;
using Nancy.Hosting.Self;
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
        private readonly int watchdogIntervalMilliseconds;

        private readonly NancyHost nancyHost;

        public Service(int watchdogIntervalMilliseconds, Uri restUri = null)
        {
            this.watchdogIntervalMilliseconds = watchdogIntervalMilliseconds;

            if (restUri != null)
            {
                nancyHost = new NancyHost(
                    new HostConfiguration
                    {
                        UrlReservations = new UrlReservations { CreateAutomatically = true }
                    }, restUri);
            }
            else
            {
                Log.Information("Rest API has not been configured");
            }
        }

        public bool Start(HostControl hostControl)
        {
            PluginManager.Instance.StartPluginsNotCurrentlyRunning();
            SetupPluginsDirectoryWatcher(PluginManager.Instance.PluginsDirectory, PluginManager.Instance.Plugins);

            var watchdogTimer = new Timer(
                PluginManager.Instance.CleanupCrashedPlugins,
                null,
                0,
                watchdogIntervalMilliseconds);

            if (nancyHost != null)
            {
                nancyHost.Start();
                Log.Information("Rest API started");
            }

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            PluginManager.Instance.StopPlugins();

            if (nancyHost != null)
            {
                nancyHost.Stop();
                Log.Information("Rest API stopped");
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

                PluginManager.Instance.StartPluginsNotCurrentlyRunning();
            };
        }

        private FileSystemEventHandler OnPluginDirectoryChanged(
            DirectoryInfo pluginsDirectory,
            List<Plugin> plugins)
        {
            return (sender, e) =>
            {
                Log.Information("Plugin directory {@AffectedDirectory} has been changed", e.FullPath);

                PluginManager.Instance.PluginDirectoryChange(e);
            };
        }

        private FileSystemEventHandler OnPluginDirectoryDeleted(List<Plugin> plugins)
        {
            return (sender, e) =>
            {
                Log.Information("Plugin has been deleted");

                PluginManager.Instance.RemovePluginsThatNoLongerExist();
            };
        }
    }
}
