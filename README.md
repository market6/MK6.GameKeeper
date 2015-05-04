# MK6.GameKeeper

GameKeeper is a Windows service that hosts one or more add-ins. It is intended that the add-ins are, themselves, long-running processes such as web services, message handlers, etc.

GameKeeper benefits these add-ins by allowing them to be updated without downtime and without impact to any other add-ins running in the instance. Also, deploying or redeploying an add-in does not require the management or installation of a Windows service, a process that can, sometimes, be tricky and problematic.

GameKeeper is built using [Topshelf](http://topshelf-project.com/) and the [Microsoft Managed Add-in Framework (MAF)](https://msdn.microsoft.com/en-us/library/gg145020%28v=vs.110%29.aspx). The GameKeeper monitors the "AddIns" folder in the MAF pipeline for new or updated add-ins. When new add-ins are found, they are started. When a new version of a running add-in is found, the running instance of the old version is stopped and an instance of the new version is started.

Add-ins are run in independent processes so that a failure by one add-in will not impact any others.

## Building Add-ins

Building add-ins is very easy. 

- Create a new class library project in a .NET project.
- Install the [MK6.GameKeeper.AddIns nuget package](https://www.nuget.org/packages/MK6.GameKeeper.AddIns) to the project
- Create a class that will be the base for your service. Make that class inherit from [MK6.GameKeeper.AddIns.GameKeeperAddIn](https://github.com/market6/MK6.GameKeeper.AddIns/blob/master/MK6.GameKeeper.AddIns/GameKeeperAddIn.cs)
- Add the [System.AddIn.AddInAttribute](https://msdn.microsoft.com/en-us/library/system.addin.addinattribute%28v=vs.110%29.aspx) to the class, providing the name and version for your plugin.
- Implement the Start, Stop, and Status methods to fulfill the GameKeeperAddIn interface

## Hints, Gotchas, ...

* The MK6.GameKeeper.AddIns package referenced by an add-in has to be the same version as that referenced by the instance of GameKeeper that hosts it. If they do not match, GameKeeper will not find the add-in and will nothing will be logged about it.
* For GameKeeper to recognize that a new version of a plugin is deployed, the name must be the same as the old version and the new version must be greater. One tactic to satisfy this is to update the version number in the AddInAttribute during MSBuild's prebuild target.
* Logging by add-ins is independent of GameKeeper's own logging. Further, GameKeeper does not provide any logging services to add-ins. It is advisable to log from add-ins to aid in debugging.

## License

See the [LICENSE](LICENSE) file for license rights and limitations (MIT).