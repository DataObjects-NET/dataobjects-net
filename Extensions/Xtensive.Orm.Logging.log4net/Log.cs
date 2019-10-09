// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.02.20

using System.Reflection;
using log4net.Core;
using log4netLogManager = log4net.LogManager;
using log4netLog = log4net.ILog;

namespace Xtensive.Orm.Logging.log4net
{
  public class Log : BaseLog
  {
    private log4netLog target;

    private Level ConvertLevel(LogLevel level)
    {
      switch (level) {
      case LogLevel.Debug:
        return Level.Debug;
      case LogLevel.Info:
        return Level.Info;
      case LogLevel.Warning:
        return Level.Warn;
      case LogLevel.Error:
        return Level.Error;
      case LogLevel.FatalError:
        return Level.Fatal;
      default:
        return Level.Info;
      }
    }
    public override bool IsLogged(LogLevel level)
    {
      return target.Logger.IsEnabledFor(ConvertLevel(level));
    }

    public override void Write(LogEventInfo info)
    {
      target.Logger.Log(target.Logger.GetType(), ConvertLevel(info.Level), info.FormattedMessage, info.Exception);
    }

    public Log(Assembly repositoryAssembly, string name)
    {
      target = log4netLogManager.GetLogger(repositoryAssembly, name);
    }
  }
}
