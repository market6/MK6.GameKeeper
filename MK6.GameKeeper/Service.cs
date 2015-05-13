using Nancy.Hosting.Self;
using Serilog;
using System;
using System.IO;
using Topshelf;

namespace MK6.GameKeeper
{
    class Service : ServiceControl
    {
        private readonly NancyHost nancyHost;

        private readonly Uri RestURI;

        public Service(DirectoryInfo pipelineDirectory, Uri restUri)
        {
            this.RestURI = restUri;

            AddInManager.Initialize(pipelineDirectory);

            if (restUri != null)
            {
                nancyHost = new NancyHost(
                    new HostConfiguration
                    {
                        UrlReservations = new UrlReservations { CreateAutomatically = true }
                    },
                    restUri);
            }
            else
            {
                Log.Information("Rest API has not been configured");
            }
        }

        public bool Start(HostControl hostControl)
        {
            AddInManager.Instance.Start();

            if (nancyHost != null)
            {
                nancyHost.Start();
                Log.Information("Rest API started on port {@RestAPIPort}", RestURI.Port);
            }

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            AddInManager.Instance.Stop();

            if (nancyHost != null)
            {
                nancyHost.Stop();
                Log.Information("Rest API stopped");
            }

            return true;
        }
    }
}
