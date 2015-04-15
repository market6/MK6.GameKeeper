using Serilog;
using System;
using System.Configuration;
using System.IO;
using Topshelf;

namespace MK6.GameKeeper
{
    class Program
    {
        const string DefaultServiceName = "GameKeeper";

        const string DefaultDisplayName = "GameKeeper";

        const string DefaultDescription = "GameKeeper";

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .CreateLogger();

            HostFactory.Run(config =>
            {
                config.Service<Service>(
                    host =>
                    {
                        var pluginDirectoryName = ConfigurationManager.AppSettings["PluginDirectory"];
                        var watchdogFrequency = int.Parse(ConfigurationManager.AppSettings["WatchdogFrequency"]);
                        var pluginsDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, pluginDirectoryName));

                        return new Service(pluginsDirectory, watchdogFrequency);
                    },
                    svc =>
                    {
                        svc.BeforeStartingService(() => Log.Information("Starting service"));
                        svc.AfterStartingService(() => Log.Information("Service started"));
                        svc.BeforeStoppingService(() => Log.Information("Stopping service"));
                        svc.AfterStoppingService(() => Log.Information("Service stopped"));
                    });

                config.RunAsLocalService();
                config.SetServiceName(ConfigurationManager.AppSettings["Name"] ?? DefaultServiceName);
                config.SetDisplayName(ConfigurationManager.AppSettings["DisplayName"] ?? DefaultDisplayName);
                config.SetDescription(ConfigurationManager.AppSettings["Description"] ?? DefaultDescription);
            });
        }

    }
}
