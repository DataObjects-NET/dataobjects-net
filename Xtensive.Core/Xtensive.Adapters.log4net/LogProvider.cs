// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.10

using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;
using log4net;
using log4net.Config;
using Xtensive.Core.Diagnostics;
using ILog=Xtensive.Core.Diagnostics.ILog;

namespace Xtensive.Adapters.log4net
{
  /// <summary>
  /// Log provider implementation for log4net.
  /// </summary>
  public sealed class LogProvider: LogProviderBase
  {
    /// <summary>
    /// Gets the <see cref="Core.Diagnostics.ILog"/> instance.
    /// </summary>
    /// <param name="realLog">The real log.</param>
    /// <returns></returns>
    protected override ILog GetLog(IRealLog realLog)
    {
      return new LogImplementation(realLog);
    }

    /// <summary>
    /// Gets the <see cref="IRealLog"/> instance.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    protected override IRealLog GetRealLog(string key)
    {
      return new RealLogImplementation(key);
    }


    // Default configuration builder

    static void EnsureLoggingConfigured()
    {
      string defaultConfig = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<log4net>
  <appender name=""ConsoleAppender"" type=""log4net.Appender.ConsoleAppender"">
    <layout type=""log4net.Layout.PatternLayout"">
      <conversionPattern value=""%5timestamp/%-4.8thread %5level %-24logger %property{indentString}%message%newline"" />
    </layout>
  </appender>

  <appender name=""NullAppender"" type=""log4net.Appender.ForwardingAppender"" >
  </appender>

  <root>
    <level value=""ALL"" />
    <appender-ref ref=""NullAppender"" />
  </root>

  <logger name=""Console"" additivity=""false"">
    <level value=""ALL"" />
    <appender-ref ref=""ConsoleAppender"" />
  </logger>
  <logger name=""Null"" additivity=""false"">
    <appender-ref ref=""NullAppender"" />
  </logger>
</log4net>
";
      XmlElement section = null;
      try {
        section = ConfigurationManager.GetSection("log4net") as XmlElement;
      }
      catch {
      }
      if (section != null)
        XmlConfigurator.Configure(section);
      if (LogManager.GetCurrentLoggers().Length==0) {
        XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(defaultConfig)));
        System.Diagnostics.Debug.Assert(LogManager.GetCurrentLoggers().Length > 0);
      }
    }

    
    // Type initializer

    static LogProvider()
    {
      EnsureLoggingConfigured();
    }
  }
}