// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.03

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core;
using Xtensive.IoC;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Log template - simplifies logging,
  /// provides support for <see cref="LogCaptureScope"/>.
  /// </summary>
  /// <typeparam name="T">Should always be the type of descendant.</typeparam>
  public class LogTemplate<T>
  {
    private static ILog instance = null;

    /// <summary>
    /// Gets the <see cref="ILog"/> this type logs to.
    /// </summary>
    public static ILog Instance {
      [DebuggerStepThrough]
      get { return instance; }
      protected set
      {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        instance = value;
      }
    }

    #region ILog-like static methods

    /// <see cref="ILogBase.IsLogged" copy="true" />
    public static bool IsLogged(LogEventTypes eventType)
    {
      return Instance.IsLogged(eventType);
    }

    /// <see cref="ILog.Debug(string,object[])" copy="true" />
    [Conditional("DEBUG")]
    public static void Debug(string format, params object[] args)
    {
      Instance.Debug(format, args);
    }

    /// <see cref="ILog.Debug(Exception,string,object[])" copy="true" />
    public static Exception Debug(Exception exception, string format, params object[] args)
    {
      return Instance.Debug(exception, format, args);
    }

    /// <see cref="ILog.Debug(Exception)" copy="true" />
    public static Exception Debug(Exception exception)
    {
      return Instance.Debug(exception);
    }

    /// <see cref="ILog.DebugRegion" copy="true" />
    public static IDisposable DebugRegion(string format, params object[] args)
    {
      return Instance.DebugRegion(format, args);
    }

    /// <see cref="ILog.Info(string,object[])" copy="true" />
    public static void Info(string format, params object[] args)
    {
      Instance.Info(format, args);
    }

    /// <see cref="ILog.Info(Exception,string,object[])" copy="true" />
    public static Exception Info(Exception exception, string format, params object[] args)
    {
      return Instance.Info(exception, format, args);
    }

    /// <see cref="ILog.Info(Exception)" copy="true" />
    public static Exception Info(Exception exception)
    {
      return Instance.Info(exception);
    }

    /// <see cref="ILog.InfoRegion" copy="true" />
    public static IDisposable InfoRegion(string format, params object[] args)
    {
      return Instance.InfoRegion(format, args);
    }

    /// <see cref="ILog.Warning(string,object[])" copy="true" />
    public static void Warning(string format, params object[] args)
    {
      Instance.Warning(format, args);
    }

    /// <see cref="ILog.Warning(Exception,string,object[])" copy="true" />
    public static Exception Warning(Exception exception, string format, params object[] args)
    {
      return Instance.Warning(exception, format, args);
    }

    /// <see cref="ILog.Warning(Exception)" copy="true" />
    public static Exception Warning(Exception exception)
    {
      return Instance.Warning(exception);
    }

    /// <see cref="ILog.Error(string,object[])" copy="true" />
    public static void Error(string format, params object[] args)
    {
      Instance.Error(format, args);
    }

    /// <see cref="ILog.Error(Exception,string,object[])" copy="true" />
    public static Exception Error(Exception exception, string format, params object[] args)
    {
      return Instance.Error(exception, format, args);
    }

    /// <see cref="ILog.Error(Exception)" copy="true" />
    public static Exception Error(Exception exception)
    {
      return Instance.Error(exception);
    }

    /// <see cref="ILog.FatalError(string,object[])" copy="true" />
    public static void FatalError(string format, params object[] args)
    {
      Instance.FatalError(format, args);
    }

    /// <see cref="ILog.FatalError(Exception,string,object[])" copy="true" />
    public static Exception FatalError(Exception exception, string format, params object[] args)
    {
      return Instance.FatalError(exception, format, args);
    }

    /// <see cref="ILog.FatalError(Exception)" copy="true" />
    public static Exception FatalError(Exception exception)
    {
      return Instance.FatalError(exception);
    }

    #endregion


    // Type initializer

    static LogTemplate()
    {
      Type t = typeof (T);
      string logName = (string)t.GetField("Name", 
        BindingFlags.Static | 
          BindingFlags.Public)
        .GetValue(null);
      string skipPrefix = "Xtensive.";
      if (logName.StartsWith(skipPrefix))
        logName = logName.Substring(skipPrefix.Length);

      Instance = LogProvider.GetLog(logName);

      if (CoreLog.IsLogged(LogEventTypes.Info))
        CoreLog.Info("{0} log initialized.", Instance);
    }
  }
}