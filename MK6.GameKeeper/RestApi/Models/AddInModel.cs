using Nancy.Helpers;

namespace MK6.GameKeeper.RestApi.Models
{
    class AddInModel
    {
        public readonly string Link;

        public readonly string Id;

        public readonly string Name;

        public readonly string Version;

        public readonly string Status;

        public AddInModel(AddIn addin)
        {
            Link = "/addins/" + addin.Id;
            Id = addin.Id;
            Name = addin.Name;
            Version = addin.Version.ToString();
            Status = addin.Status.ToString();
        }
    }
}
