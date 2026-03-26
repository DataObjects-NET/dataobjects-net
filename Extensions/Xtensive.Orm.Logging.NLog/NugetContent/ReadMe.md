Xtensive.Orm.Logging.NLog
=========================

Summary
-------
The extension provides integration points between DataObjects.Net internal logging system and NLog.

Prerequisites
-------------

DataObjects.Net 7.1.x (http://dataobjects.net)
NLog 4.5 or later (http://nlog-project.org)

Implementation
--------------

Set up log provider in Xtensive.Orm configuration section

```xml
  <Xtensive.Orm>
    <domains>
      <domain ... >
      </domain>
    </domains>
    <logging provider="Xtensive.Orm.Logging.NLog.LogProvider, Xtensive.Orm.Logging.NLog">
  </Xtensive.Orm>
```

Configure NLog (https://github.com/nlog/nlog/wiki/Tutorial), e.g.:

```xml
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
```

You can choose from the following predefined internal loggers:
 - "Xtensive.Orm"
 - "Xtensive.Orm.Upgrade"
 - "Xtensive.Orm.Building"
 - "Xtensive.Orm.Core"
 - "Xtensive.Orm.Sql"
