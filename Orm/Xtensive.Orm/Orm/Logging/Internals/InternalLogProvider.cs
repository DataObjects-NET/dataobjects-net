// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Logging
{
  internal sealed class InternalLogProvider : LogProvider
  {
    private static readonly StringComparer Comparer = StringComparer.InvariantCultureIgnoreCase;

    private LogWriter defaultWriter;
    private readonly Dictionary<string, BaseLog> configuredLogs;

    public override BaseLog GetLog(string logName)
    {
      logName = logName.Trim();
      BaseLog log;
      if (configuredLogs.TryGetValue(logName, out log))
        return log;
      return CreateLog(logName, defaultWriter);
    }

    private bool IsDefault(string logName)
    {
      return Comparer.Compare(logName, "*")==0;
    }

    private static IEnumerable<string> ParseSources(string sources)
    {
      if (sources==null)
        return Enumerable.Empty<string>();

      return sources.Split(',')
        .Select(item => item.Trim())
        .Where(item => item!=string.Empty);
    }

    private void RegisterLogs(string sources, LogWriter writer)
    {
      foreach (var source in ParseSources(sources))
        configuredLogs[source] = CreateLog(source, writer);
    }

    private BaseLog CreateLog(string name, LogWriter writer)
    {
      return writer==null
        ? (BaseLog) new NullLog(name)
        : new InternalLog(name, writer);
    }

    private LogWriter CreateWriter(string target)
    {
      if (Comparer.Compare(target, "Console")==0)
        return new ConsoleWriter();
      if (Comparer.Compare(target, "DebugOnlyConsole")==0)
        return new DebugOnlyConsoleWriter();
      if (Comparer.Compare(target, "None")==0)
        return null;
      if (Path.IsPathRooted(target))
        return new FileWriter(target);
      return new FileWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, target));
    }

    private void ProcessLogGroup(string target, IEnumerable<LogConfiguration> configurations)
    {
      if (target==string.Empty)
        return;
      var writer = CreateWriter(target);
      foreach (var log in configurations)
        if (IsDefault(log.Source))
          defaultWriter = writer;
        else
          RegisterLogs(log.Source, writer);
    }

    public InternalLogProvider()
    {
      configuredLogs = new Dictionary<string, BaseLog>();
    }

    public InternalLogProvider(IEnumerable<LogConfiguration> logConfigurations)
    {
      configuredLogs = new Dictionary<string, BaseLog>();
      var logGroups = logConfigurations.GroupBy(e => (e.Target ?? string.Empty).Trim());
      foreach (var item in logGroups)
        ProcessLogGroup(item.Key, item);
    }
  }
}