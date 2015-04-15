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
                        svc.BeforeStartingService(() => Console.WriteLine("Starting service"));
                        svc.AfterStartingService(() => Console.WriteLine("Service started"));
                        svc.BeforeStoppingService(() => Console.WriteLine("Stopping service"));
                        svc.AfterStoppingService(() => Console.WriteLine("Service stopped"));
                    });

                config.RunAsLocalService();
                config.SetServiceName(ConfigurationManager.AppSettings["Name"] ?? DefaultServiceName);
                config.SetDisplayName(ConfigurationManager.AppSettings["DisplayName"] ?? DefaultDisplayName);
                config.SetDescription(ConfigurationManager.AppSettings["Description"] ?? DefaultDescription);
            });
        }

    }
}
