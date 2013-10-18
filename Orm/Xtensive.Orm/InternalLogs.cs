using System;
using Xtensive.Orm.Logging;

namespace Xtensive
{
  internal static class OrmLog
  {
    private static string Name = "Xtensive.Orm";
    private static readonly BaseLog instance;

    public static BaseLog Instance {
      get { return instance; }
    }

    public static bool IsLogged(LogEventTypes type)
    {
      return instance.IsLogged(type);
    }

    public static IDisposable DebugRegion(string format, params object[] args)
    {
      return instance.DebugRegion(format, args);
    }

    public static void Debug(string format, params object[] args)
    {
      instance.Debug(format, args);
    }

    public static Exception Debug(Exception exception, string format, params object[] args)
    {
      instance.Debug(format, args, exception);
      return exception;
    }

    public static Exception Debug(Exception exception)
    {
      instance.Debug(null, null, exception);
      return exception;
    }

    public static IDisposable InfoRegion(string format, params object[] args)
    {
      return instance.InfoRegion(format, args);
    }

    public static void Info(string format, params object[] args)
    {
      instance.Info(format, args);
    }

    public static Exception Info(Exception exception, string format, params object[] args)
    {
      instance.Info(format, args, exception);
      return exception;
    }

    public static Exception Info(Exception exception)
    {
      instance.Info(null, null, exception);
      return exception;
    }

    public static void Warning(string format, params object[] args)
    {
      instance.Warn(format, args);
    }

    public static Exception Warning(Exception exception, string format, params object[] args)
    {
      instance.Warn(format, args, exception);
      return exception;
    }

    public static Exception Warning(Exception exception)
    {
      instance.Warn(null, null, exception);
      return exception;
    }

    public static void Error(string format, params object[] args)
    {
      instance.Error(format, args);
    }

    public static Exception Error(Exception exception, string format, params object[] args)
    {
      instance.Error(format, args, exception);
      return exception;
    }

    public static Exception Error(Exception exception)
    {
      instance.Error(null, null, exception);
      return exception;
    }

    public static void FatalError(string format, params object[] args)
    {
      instance.Fatal(format, args);
    }

    public static Exception FatalError(Exception exception, string format, params object[] args)
    {
      instance.Fatal(format, args, exception);
      return exception;
    }

    public static Exception FatalError(Exception exception)
    {
      instance.Fatal(null, null, exception);
      return exception;
    }

    static OrmLog()
    {
      var manager = LogManager.Default;
      manager.AutoInitialize();
      instance = manager.GetLog(Name);
    }
  }

  internal static class UpgradeLog
  {
    private static string Name = "Xtensive.Orm.Upgrade";
    private static readonly BaseLog instance;

    public static BaseLog Instance {
      get { return instance; }
    }

    public static bool IsLogged(LogEventTypes type)
    {
      return instance.IsLogged(type);
    }

    public static IDisposable DebugRegion(string format, params object[] args)
    {
      return instance.DebugRegion(format, args);
    }

    public static void Debug(string format, params object[] args)
    {
      instance.Debug(format, args);
    }

    public static Exception Debug(Exception exception, string format, params object[] args)
    {
      instance.Debug(format, args, exception);
      return exception;
    }

    public static Exception Debug(Exception exception)
    {
      instance.Debug(null, null, exception);
      return exception;
    }

    public static IDisposable InfoRegion(string format, params object[] args)
    {
      return instance.InfoRegion(format, args);
    }

    public static void Info(string format, params object[] args)
    {
      instance.Info(format, args);
    }

    public static Exception Info(Exception exception, string format, params object[] args)
    {
      instance.Info(format, args, exception);
      return exception;
    }

    public static Exception Info(Exception exception)
    {
      instance.Info(null, null, exception);
      return exception;
    }

    public static void Warning(string format, params object[] args)
    {
      instance.Warn(format, args);
    }

    public static Exception Warning(Exception exception, string format, params object[] args)
    {
      instance.Warn(format, args, exception);
      return exception;
    }

    public static Exception Warning(Exception exception)
    {
      instance.Warn(null, null, exception);
      return exception;
    }

    public static void Error(string format, params object[] args)
    {
      instance.Error(format, args);
    }

    public static Exception Error(Exception exception, string format, params object[] args)
    {
      instance.Error(format, args, exception);
      return exception;
    }

    public static Exception Error(Exception exception)
    {
      instance.Error(null, null, exception);
      return exception;
    }

    public static void FatalError(string format, params object[] args)
    {
      instance.Fatal(format, args);
    }

    public static Exception FatalError(Exception exception, string format, params object[] args)
    {
      instance.Fatal(format, args, exception);
      return exception;
    }

    public static Exception FatalError(Exception exception)
    {
      instance.Fatal(null, null, exception);
      return exception;
    }

    static UpgradeLog()
    {
      var manager = LogManager.Default;
      manager.AutoInitialize();
      instance = manager.GetLog(Name);
    }
  }

  internal static class BuildLog
  {
    private static string Name = "Xtensive.Orm.Building";
    private static readonly BaseLog instance;

    public static BaseLog Instance {
      get { return instance; }
    }

    public static bool IsLogged(LogEventTypes type)
    {
      return instance.IsLogged(type);
    }

    public static IDisposable DebugRegion(string format, params object[] args)
    {
      return instance.DebugRegion(format, args);
    }

    public static void Debug(string format, params object[] args)
    {
      instance.Debug(format, args);
    }

    public static Exception Debug(Exception exception, string format, params object[] args)
    {
      instance.Debug(format, args, exception);
      return exception;
    }

    public static Exception Debug(Exception exception)
    {
      instance.Debug(null, null, exception);
      return exception;
    }

    public static IDisposable InfoRegion(string format, params object[] args)
    {
      return instance.InfoRegion(format, args);
    }

    public static void Info(string format, params object[] args)
    {
      instance.Info(format, args);
    }

    public static Exception Info(Exception exception, string format, params object[] args)
    {
      instance.Info(format, args, exception);
      return exception;
    }

    public static Exception Info(Exception exception)
    {
      instance.Info(null, null, exception);
      return exception;
    }

    public static void Warning(string format, params object[] args)
    {
      instance.Warn(format, args);
    }

    public static Exception Warning(Exception exception, string format, params object[] args)
    {
      instance.Warn(format, args, exception);
      return exception;
    }

    public static Exception Warning(Exception exception)
    {
      instance.Warn(null, null, exception);
      return exception;
    }

    public static void Error(string format, params object[] args)
    {
      instance.Error(format, args);
    }

    public static Exception Error(Exception exception, string format, params object[] args)
    {
      instance.Error(format, args, exception);
      return exception;
    }

    public static Exception Error(Exception exception)
    {
      instance.Error(null, null, exception);
      return exception;
    }

    public static void FatalError(string format, params object[] args)
    {
      instance.Fatal(format, args);
    }

    public static Exception FatalError(Exception exception, string format, params object[] args)
    {
      instance.Fatal(format, args, exception);
      return exception;
    }

    public static Exception FatalError(Exception exception)
    {
      instance.Fatal(null, null, exception);
      return exception;
    }

    static BuildLog()
    {
      var manager = LogManager.Default;
      manager.AutoInitialize();
      instance = manager.GetLog(Name);
    }
  }

  internal static class SqlLog
  {
    private static string Name = "Xtensive.Storage.Providers.Sql";
    private static readonly BaseLog instance;

    public static BaseLog Instance {
      get { return instance; }
    }

    public static bool IsLogged(LogEventTypes type)
    {
      return instance.IsLogged(type);
    }

    public static IDisposable DebugRegion(string format, params object[] args)
    {
      return instance.DebugRegion(format, args);
    }

    public static void Debug(string format, params object[] args)
    {
      instance.Debug(format, args);
    }

    public static Exception Debug(Exception exception, string format, params object[] args)
    {
      instance.Debug(format, args, exception);
      return exception;
    }

    public static Exception Debug(Exception exception)
    {
      instance.Debug(null, null, exception);
      return exception;
    }

    public static IDisposable InfoRegion(string format, params object[] args)
    {
      return instance.InfoRegion(format, args);
    }

    public static void Info(string format, params object[] args)
    {
      instance.Info(format, args);
    }

    public static Exception Info(Exception exception, string format, params object[] args)
    {
      instance.Info(format, args, exception);
      return exception;
    }

    public static Exception Info(Exception exception)
    {
      instance.Info(null, null, exception);
      return exception;
    }

    public static void Warning(string format, params object[] args)
    {
      instance.Warn(format, args);
    }

    public static Exception Warning(Exception exception, string format, params object[] args)
    {
      instance.Warn(format, args, exception);
      return exception;
    }

    public static Exception Warning(Exception exception)
    {
      instance.Warn(null, null, exception);
      return exception;
    }

    public static void Error(string format, params object[] args)
    {
      instance.Error(format, args);
    }

    public static Exception Error(Exception exception, string format, params object[] args)
    {
      instance.Error(format, args, exception);
      return exception;
    }

    public static Exception Error(Exception exception)
    {
      instance.Error(null, null, exception);
      return exception;
    }

    public static void FatalError(string format, params object[] args)
    {
      instance.Fatal(format, args);
    }

    public static Exception FatalError(Exception exception, string format, params object[] args)
    {
      instance.Fatal(format, args, exception);
      return exception;
    }

    public static Exception FatalError(Exception exception)
    {
      instance.Fatal(null, null, exception);
      return exception;
    }

    static SqlLog()
    {
      var manager = LogManager.Default;
      manager.AutoInitialize();
      instance = manager.GetLog(Name);
    }
  }

  internal static class CoreLog
  {
    private static string Name = "Xtensive.Orm.Core";
    private static readonly BaseLog instance;

    public static BaseLog Instance {
      get { return instance; }
    }

    public static bool IsLogged(LogEventTypes type)
    {
      return instance.IsLogged(type);
    }

    public static IDisposable DebugRegion(string format, params object[] args)
    {
      return instance.DebugRegion(format, args);
    }

    public static void Debug(string format, params object[] args)
    {
      instance.Debug(format, args);
    }

    public static Exception Debug(Exception exception, string format, params object[] args)
    {
      instance.Debug(format, args, exception);
      return exception;
    }

    public static Exception Debug(Exception exception)
    {
      instance.Debug(null, null, exception);
      return exception;
    }

    public static IDisposable InfoRegion(string format, params object[] args)
    {
      return instance.InfoRegion(format, args);
    }

    public static void Info(string format, params object[] args)
    {
      instance.Info(format, args);
    }

    public static Exception Info(Exception exception, string format, params object[] args)
    {
      instance.Info(format, args, exception);
      return exception;
    }

    public static Exception Info(Exception exception)
    {
      instance.Info(null, null, exception);
      return exception;
    }

    public static void Warning(string format, params object[] args)
    {
      instance.Warn(format, args);
    }

    public static Exception Warning(Exception exception, string format, params object[] args)
    {
      instance.Warn(format, args, exception);
      return exception;
    }

    public static Exception Warning(Exception exception)
    {
      instance.Warn(null, null, exception);
      return exception;
    }

    public static void Error(string format, params object[] args)
    {
      instance.Error(format, args);
    }

    public static Exception Error(Exception exception, string format, params object[] args)
    {
      instance.Error(format, args, exception);
      return exception;
    }

    public static Exception Error(Exception exception)
    {
      instance.Error(null, null, exception);
      return exception;
    }

    public static void FatalError(string format, params object[] args)
    {
      instance.Fatal(format, args);
    }

    public static Exception FatalError(Exception exception, string format, params object[] args)
    {
      instance.Fatal(format, args, exception);
      return exception;
    }

    public static Exception FatalError(Exception exception)
    {
      instance.Fatal(null, null, exception);
      return exception;
    }

    static CoreLog()
    {
      var manager = LogManager.Default;
      manager.AutoInitialize();
      instance = manager.GetLog(Name);
    }
  }

  internal static class TestLog
  {
    private static string Name = "Xtensive.Orm.Tests";
    private static readonly BaseLog instance;

    public static BaseLog Instance {
      get { return instance; }
    }

    public static bool IsLogged(LogEventTypes type)
    {
      return instance.IsLogged(type);
    }

    public static IDisposable DebugRegion(string format, params object[] args)
    {
      return instance.DebugRegion(format, args);
    }

    public static void Debug(string format, params object[] args)
    {
      instance.Debug(format, args);
    }

    public static Exception Debug(Exception exception, string format, params object[] args)
    {
      instance.Debug(format, args, exception);
      return exception;
    }

    public static Exception Debug(Exception exception)
    {
      instance.Debug(null, null, exception);
      return exception;
    }

    public static IDisposable InfoRegion(string format, params object[] args)
    {
      return instance.InfoRegion(format, args);
    }

    public static void Info(string format, params object[] args)
    {
      instance.Info(format, args);
    }

    public static Exception Info(Exception exception, string format, params object[] args)
    {
      instance.Info(format, args, exception);
      return exception;
    }

    public static Exception Info(Exception exception)
    {
      instance.Info(null, null, exception);
      return exception;
    }

    public static void Warning(string format, params object[] args)
    {
      instance.Warn(format, args);
    }

    public static Exception Warning(Exception exception, string format, params object[] args)
    {
      instance.Warn(format, args, exception);
      return exception;
    }

    public static Exception Warning(Exception exception)
    {
      instance.Warn(null, null, exception);
      return exception;
    }

    public static void Error(string format, params object[] args)
    {
      instance.Error(format, args);
    }

    public static Exception Error(Exception exception, string format, params object[] args)
    {
      instance.Error(format, args, exception);
      return exception;
    }

    public static Exception Error(Exception exception)
    {
      instance.Error(null, null, exception);
      return exception;
    }

    public static void FatalError(string format, params object[] args)
    {
      instance.Fatal(format, args);
    }

    public static Exception FatalError(Exception exception, string format, params object[] args)
    {
      instance.Fatal(format, args, exception);
      return exception;
    }

    public static Exception FatalError(Exception exception)
    {
      instance.Fatal(null, null, exception);
      return exception;
    }

    static TestLog()
    {
      var manager = LogManager.Default;
      manager.AutoInitialize();
      instance = manager.GetLog(Name);
    }
  }

}
 