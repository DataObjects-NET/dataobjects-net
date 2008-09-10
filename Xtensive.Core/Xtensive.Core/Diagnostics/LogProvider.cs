// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.10

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using log4net;
using log4net.Config;
using log4net.Util;
using Xtensive.Core.Diagnostics.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Diagnostics
{
  /// <summary>
  /// Provides (creates or resolves) <see cref="ILog"/> instances by their name.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="SingletonDocTemplate" copy="true" /></para>
  /// </remarks>
  public sealed class LogProvider:
    IProvider<string, ILog>,
    ISynchronizable
  {
    private static LogProvider instance;
    private object syncRoot = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private Dictionary<string, ILog> logs = new Dictionary<string, ILog>();

    
    // Static methods
    
    /// <see cref="SingletonDocTemplate.Instance" copy="true"/>
    public static LogProvider Instance {
      [DebuggerStepThrough]
      get { return instance; }
    }

    /// <summary>
    /// Gets the <see cref="ILog"/> object by its <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to get the log for.</param>
    /// <returns>The <see cref="ILog"/> object.</returns>
    public static ILog GetLog(string key)
    {
      return Instance[key];
    }

    /// <summary>
    /// Gets <see cref="ILog"/> object forwarding logging messages to console.
    /// </summary>
    public static ILog ConsoleLog
    {
      get { return LogProvider.GetLog("Console"); }
    }

    /// <summary>
    /// Gets <see cref="ILog"/> object forwarding logging messages to nothing.
    /// </summary>
    public static ILog NullLog
    {
      get { return LogProvider.GetLog("Null"); }
    }


    // Instance methods

    /// <summary>
    /// Gets the <see cref="ILog"/> object by the specified key.
    /// </summary>
    public ILog this[string key]
    {
      get {
        using (Locker.ReadRegion(SyncRoot)) {
          ILog log;
          if (!logs.TryGetValue(key, out log)) {
            log = CreateLog(key);
            logs[key] = log;
          }
          return log;
        }
      }
    }

    private ILog CreateLog(string key)
    {
      IRealLog realLog = new RealLogImplementation(key);
      return new LogImplementation(realLog);
    }

    /// <inheritdoc/>
    public bool IsSynchronized
    {
      [DebuggerStepThrough]
      get { return true; }
    }

    /// <inheritdoc/>
    public object SyncRoot
    {
      [DebuggerStepThrough]
      get { return syncRoot; }
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
      if (LogManager.GetCurrentLoggers().Length==0) {
        XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(defaultConfig)));
        System.Diagnostics.Debug.Assert(LogManager.GetCurrentLoggers().Length > 0);
      }
    }

    
    // Type initializer

    static LogProvider()
    {
      EnsureLoggingConfigured();
      instance = new LogProvider();
    }
  }
}