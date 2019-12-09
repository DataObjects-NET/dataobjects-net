# DataObjects.Net Extensions

DataObjects.Net Extensions are small projects that extend standard DataObjects.Net. 

Official extensions:

- [Bulk Operations]((https://github.com/DataObjects-NET/dataobjects-net/blob/master/Documentation/Extensions/BulkOperations.md))
- [Localization]((https://github.com/DataObjects-NET/dataobjects-net/blob/master/Documentation/Extensions/Localization.md))
- [Logging extension for log4net]((https://github.com/DataObjects-NET/dataobjects-net/blob/master/Documentation/Extensions/log4net-integration-extension.md))
- [Logging extension for NLog]((https://github.com/DataObjects-NET/dataobjects-net/blob/master/Documentation/Extensions/NLog-integration-extension.md))
- [Reprocessing]((https://github.com/DataObjects-NET/dataobjects-net/blob/master/Documentation/Extensions/Reprocessing.md))
- [Tracking]((https://github.com/DataObjects-NET/dataobjects-net/blob/master/Documentation/Extensions/Tracking.md))
- [Tracking]((https://github.com/DataObjects-NET/dataobjects-net/blob/master/Documentation/Extensions/Tracking.md))
- [Web extension]((https://github.com/DataObjects-NET/dataobjects-net/blob/master/Documentation/Extensions/Web.md))

Each extension has a corresponding Nuget package so they can be installed separately or in any combination

    dotnet add package Xtensive.Orm.BulkOperations
    dotnet add package Xtensive.Orm.Localization
    dotnet add package Xtensive.Orm.Logging.log4net
    dotnet add package Xtensive.Orm.Logging.NLog
    dotnet add package Xtensive.Orm.Reprocessing
    dotnet add package Xtensive.Orm.Security
    dotnet add package Xtensive.Orm.Tracking
    dotnet add package Xtensive.Orm.Web

Use the --version option to specify preview version to install.