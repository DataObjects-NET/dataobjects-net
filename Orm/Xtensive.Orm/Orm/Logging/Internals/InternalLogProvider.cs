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
    private readonly StringComparer comparer;
    private readonly Dictionary<string, BaseLog> logs;
    private BaseLog defaultLog;

    public override BaseLog GetLog(string logName)
    {
      BaseLog log;
      if (logs.TryGetValue(logName, out log))
        return log;
      return defaultLog;
    }

    private bool IsDefaultLog(string logName)
    {
      return comparer.Compare(logName, "*")==0;
    }

    private void ProcessLogGroup(KeyValuePair<string, List<LogConfiguration>> targetGroup)
    {
      var writer = GetLogWriter(targetGroup.Key);
      if (writer==null) {
        CreateNullLogs(targetGroup.Value);
        return;
      }
      foreach (var log in targetGroup.Value) {
        CreateLogs(log.Source, writer);
      }
    }

    private void CreateNullLogs(IEnumerable<LogConfiguration> configurations)
    {
      var nullLog = (defaultLog is NullLog) ? defaultLog : new NullLog();
      foreach (var logConfiguration in configurations) {
        if (IsDefaultLog(logConfiguration.Source)) {
          defaultLog = nullLog;
          continue;
        }
        foreach (var source in GetLogSources(logConfiguration.Source))
          logs[source] = nullLog;
      }
    }

    private static IEnumerable<string> GetLogSources(string source)
    {
      return source.Replace(" ", "").Split(',');
    }

    private void CreateLogs(string source, ILogWriter writer)
    {
      if (IsDefaultLog(source)) {
        defaultLog = new InternalLog("<default>", writer);
        return;
      }
      foreach (var s in GetLogSources(source))
        logs[s] = new InternalLog(s, writer);
    }

    private ILogWriter GetLogWriter(string target)
    {
      if (comparer.Compare(target, "Console")==0)
        return new ConsoleWriter();
      if (comparer.Compare(target, "DebugOnlyConsole")==0)
        return new DebugOnlyConsoleWriter();
      if (comparer.Compare(target, "None")==0)
        return null;
      if (Path.IsPathRooted(target))
        return new FileWriter(target);
      return new FileWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, target));
    }

    public InternalLogProvider()
    {
      logs = new Dictionary<string, BaseLog>();
      defaultLog = new NullLog();
      comparer = StringComparer.InvariantCultureIgnoreCase;
    }

    public InternalLogProvider(IEnumerable<LogConfiguration> logConfigurations)
    {
      logs = new Dictionary<string, BaseLog>();
      defaultLog = new NullLog();
      comparer = StringComparer.InvariantCultureIgnoreCase;
      var targetGroups = logConfigurations.GroupBy(e => e.Target).ToDictionary(e => e.Key, e => e.ToList());
      foreach (var targetGroup in targetGroups) {
        ProcessLogGroup(targetGroup);
      }
    }
  }
}