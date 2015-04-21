using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;

namespace MK6.GameKeeper.RestApi
{
    public class ServiceControlModule : NancyModule
    {
        public ServiceControlModule()
        {
            Get["/plugins"] = p => GetPlugins();
            Get["/plugins/{id}"] = p => GetPlugin(p.id);
            Post["/plugins/{id}/start"] = p => StartPlugin(p.id);
            Post["/plugins/{id}/stop"] = p => StopPlugin(p.id);
        }

        public dynamic GetPlugin(string pluginId)
        {
            var plugin = PluginManager.Instance.Plugins.SingleOrDefault(x => x.Id.Equals(pluginId, StringComparison.OrdinalIgnoreCase));
            if(plugin == null)
                return HttpStatusCode.NotFound;

            return new PluginModel(plugin);
        }

        public dynamic GetPlugins()
        {
            var plugins = PluginManager.Instance.Plugins.Select(x => new PluginModel(x));
            return plugins;
        }

        public dynamic StartPlugin(string pluginId)
        {
            var plugin = PluginManager.Instance.StartPlugin(pluginId);
            if(plugin == null)
                return HttpStatusCode.NotFound;

            return new PluginModel(plugin);

        }

        public dynamic StopPlugin(string pluginId)
        {
            var plugin = PluginManager.Instance.Plugins.SingleOrDefault(x => x.Id.Equals(pluginId, StringComparison.OrdinalIgnoreCase));
            if (plugin == null)
                return HttpStatusCode.NotFound;

            PluginManager.Instance.StopPlugins(new[] {plugin});
            return HttpStatusCode.OK;
        }

//        public Response GetPlugins()
//        {
//            Response resp = @"
//{
//    ""_links"": {
//        ""self"": { ""href"": ""/plugins"" }
//    },
//    ""Loaded"": 1,
//    ""Running"": 1,
//    ""Stopped"": 0,
//    ""_embedded"": {
//        ""Plugins"": [{
//            ""_links"": {
//                ""self"": { ""href"": ""/plugins/MyPlugin"" }
//            },
//            ""Name"": ""MyPlugin"",
//            ""Path"": ""C:\\gamekeeper\\plugins\\myplugin"",
//            ""Status"": ""Running"",
//            ""StartedOn"": ""4/20/2015 11:40 AM""            
//        }]
//    }
//}";
//            resp.ContentType = "application/json";

//            return resp;
//        }
    }
}
