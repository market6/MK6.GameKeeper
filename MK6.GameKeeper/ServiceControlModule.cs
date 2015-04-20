using System;
using Nancy;

namespace MK6.GameKeeper
{
    public class ServiceControlModule : NancyModule
    {
        public ServiceControlModule()
        {
            Get["plugins"] = p => GetPlugins();
        }

        public Response GetPlugins()
        {
            Response resp = @"
{
    ""_links"": {
        ""self"": { ""href"": ""/plugins"" }
    },
    ""Loaded"": 1,
    ""Running"": 1,
    ""Stopped"": 0,
    ""_embedded"": {
        ""Plugins"": [{
            ""Name"": ""MyPlugin"",
            ""Path"": ""C:\\gamekeeper\\plugins\\myplugin"",
            ""Status"": ""Running"",
            ""StartedOn"": ""4/20/2015 11:40 AM""            
        }]
    }
}";
            resp.ContentType = "application/json";

            return resp;
        }
    }
}
