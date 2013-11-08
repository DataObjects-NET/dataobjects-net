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

    private bool LogIsDefault(string logName)
    {
      return comparer.Compare(logName, "*")==0;
    }

    private void HandleTargetGroup(KeyValuePair<string, List<LogConfiguration>> targetGroup)
    {
      var writer = GetLogWriterByTarget(targetGroup.Key);
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
        if (LogIsDefault(logConfiguration.Source)) {
          defaultLog = nullLog;
          continue;
        }
        var sources = logConfiguration.Source.Replace(" ", "").Split(',');
        foreach (var source in sources) {
          logs[source] = nullLog;
        }
      }
    }

    private void CreateLogs(string source, ILogWriter writer)
    {
      if (LogIsDefault(source)) {
        defaultLog = new Log("DefaultLog", writer);
        return;
      }
      var sources = source.Replace(" ","").Split(',');
      foreach (var s in sources) {
        logs[s] = new Log(s, writer);
      }
    }

    private ILogWriter GetLogWriterByTarget(string target)
    {
      if (comparer.Compare(target, "Console")==0)
        return new ConsoleWriter();
      if (comparer.Compare(target, "DebugOnlyConsole")==0)
        return new DebugOnlyConsoleWriter();
      if (comparer.Compare(target, "None")==0)
        return null;
      if (Path.IsPathRooted(target))
        return new FileWriter(target);
      var fullpath = Path.GetFullPath(target);
      return new FileWriter(fullpath);
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
        HandleTargetGroup(targetGroup);
      }
    }
  }
}