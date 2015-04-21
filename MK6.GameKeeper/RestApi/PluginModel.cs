namespace MK6.GameKeeper.RestApi
{
    public class PluginModel
    {
        public readonly string Id;

        public readonly string Directory;

        public readonly string Exe;

        public readonly string Thread;

        public readonly string Domain;

        public PluginModel(Plugin plugin)
        {
            Id = plugin.Id;
            Directory = plugin.Directory.FullName;
            Exe = plugin.Exe.Name;
            Thread = plugin.Thread.Name ?? plugin.Thread.ManagedThreadId.ToString();
            Domain = plugin.Domain.FriendlyName;
        }
    }
}
