using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Xtensive.Orm.Configuration;

[assembly: InternalsVisibleTo("Xtensive.Orm.Tests.Core, PublicKey=" +
"0024000004800000940000000602000000240000525341310004000001000100fbdd689d62e9c6" +
"7bb6356267f95e0b58d478cf56393c4f060fbaff42a9686272e37009ab71bfa2e41046e952f389" +
"f37c6a033d1a2a5354fc97226fc469128e49e6a479ac5d1dd69d7da5607d0dc4ede0765d477745" +
"1034dc3a15f1532d010db3e633e62fc5e67a3ed175457acb9dc6c9d39ccc8ecfdaae62df34d488" +
"c45009b2")]
namespace Xtensive.Orm.Logging.Internals
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