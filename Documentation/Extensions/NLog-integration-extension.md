# NLog integration extension

The extension provides integration points between DataObjects.Net internal logging system and NLog.

### Installation

The extension is available on Nuget

    dotnet add package Xtensive.Orm.Logging.NLog

### Usage

Set up log provider in `Xtensive.Orm` configuration section

      <Xtensive.Orm>
        <domains>
          <domain ... >
          </domain>
        </domains>
        <logging provider="Xtensive.Orm.Logging.NLog.LogProvider, Xtensive.Orm.Logging.NLog">
      </Xtensive.Orm>

then configure NLog ([NLog documentation](https://github.com/nlog/nlog/wiki/Tutorial)), e.g.:

    <?xml version="1.0" encoding="utf-8" ?>
    <nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

      <targets>
        <target name="console" xsi:type="Console" />
      </targets>

      <rules>
        <logger name="Xtensive.Orm" minlevel="Debug" writeTo="console" />
      </rules>
    </nlog>

Logger name "Xtensive.Orm" corresponds with DataObjects.Net internal logger. All internal loggers available:

 - Xtensive.Orm
 - Xtensive.Orm.Upgrade
 - Xtensive.Orm.Building
 - Xtensive.Orm.Core
 - Xtensive.Orm.Sql