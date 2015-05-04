using MK6.GameKeeper.RestApi.Models;
using Nancy;
using System;
using System.Linq;

namespace MK6.GameKeeper.RestApi
{
    public class AddInsModule : NancyModule
    {
        private readonly AddInManager addInManager;

        public AddInsModule()
        {
            addInManager = AddInManager.Instance;

            Get["/addins"] = p =>
                addInManager.AddIns
                    .Select(x => new AddInModel(x))
                    .ToArray();

            Get["/addins/{id}"] = p =>
            {
                var plugin = GetAddInById(p.id.ToString());

                if (plugin == null)
                {
                    return HttpStatusCode.NotFound;
                }

                return new AddInModel(plugin);
            };
            Post["/addins/{id}/start"] = p =>
            {
                var addin = GetAddInById(p.id.ToString());

                if (addin == null)
                {
                    return HttpStatusCode.NotFound;
                }

                addin.Start();

                return HttpStatusCode.OK;
            };
            Post["/addins/{id}/stop"] = p =>
            {
                var addin = GetAddInById(p.id.ToString());

                if (addin == null)
                {
                    return HttpStatusCode.NotFound;
                }

                addin.Stop();

                return HttpStatusCode.OK;
            };
        }

        private AddIn GetAddInById(string id)
        {
            return addInManager.AddIns
                .SingleOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
    }
}
