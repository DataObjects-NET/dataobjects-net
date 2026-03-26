Xtensive.Orm.Logging.log4net
============================

Summary
-------
The extension provides integration points between DataObjects.Net internal logging system and log4net.

Prerequisites
-------------
DataObjects.Net 7.1.x (http://dataobjects.net)
log4net 2.0.10 or later (http://logging.apache.org/log4net/)

Implementation
--------------

Set up log provider in Xtensive.Orm configuration section

```xml
  <Xtensive.Orm>
    <domains>
      <domain ... >
      </domain>
    </domains>
    <logging provider="Xtensive.Orm.Logging.log4net.LogProvider, Xtensive.Orm.Logging.log4net">
  </Xtensive.Orm>
```

Configure log4net (http://logging.apache.org/log4net/release/manual/configuration.html), e.g.:

```xml
  <?xml version="1.0" encoding="utf-8" ?>
  <configuration>
    <configSections>
      ...
      <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler,log4net"/>
    </configSections>
    <Xtensive.Orm>
      <domains>
        <domain ... >
        </domain>
      </domains>
      <logging provider="Xtensive.Orm.Logging.NLog.LogProvider, Xtensive.Orm.Logging.log4net">
    </Xtensive.Orm>
  
    <log4net>
      <appender name="ConsoleAppernder" type="log4net.Appender.ConsoleAppender">
        <layout type="log4net.Layout.PatternLayout">
          <conversionPattern value="%date [%thread] %-5level %logger %message%newline" />
        </layout>
      </appender>
    
      <logger name="Xtensive.Orm">
        <level value="ALL" />
        <appender-ref ref="ConsoleAppernder" />
      </logger>
    </log4net>
  </configuration>
```

You can choose from the following predefined internal loggers:
 - "Xtensive.Orm"
 - "Xtensive.Orm.Upgrade"
 - "Xtensive.Orm.Building"
 - "Xtensive.Orm.Core"
 - "Xtensive.Orm.Sql"
