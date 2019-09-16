// Copyright (C) 2003-2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2013.12.13

using NLog;
using NLogLevel = NLog.LogLevel;
using NLogManager = NLog.LogManager;

namespace Xtensive.Orm.Logging.NLog
{
  public class Log : BaseLog
  {
    private readonly Logger target;

    private static NLogLevel ConvertLevel(LogLevel level)
    {
      switch (level) {
      case LogLevel.Debug:
        return NLogLevel.Debug;
      case LogLevel.Error:
        return NLogLevel.Error;
      case LogLevel.FatalError:
        return NLogLevel.Fatal;
      case LogLevel.Warning:
        return NLogLevel.Warn;
      default:
        return NLogLevel.Info;
      }
    }

    public override bool IsLogged(LogLevel level)
    {
      return target.IsEnabled(ConvertLevel(level));
    }

    public override void Write(LogEventInfo info)
    {
      if (info.Exception!=null)
        target.Log(ConvertLevel(info.Level), info.Exception, info.FormattedMessage);
      else
        target.Log(ConvertLevel(info.Level), info.FormattedMessage);
    }

    public Log(string name)
    {
      target = NLogManager.GetLogger(name);
    }
  }
}