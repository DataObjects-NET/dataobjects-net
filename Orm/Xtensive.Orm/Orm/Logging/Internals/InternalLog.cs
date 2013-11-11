// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Logging
{

  internal sealed class InternalLog : BaseLog
  {
    private readonly ILogWriter writer;

    public override bool IsLogged(LogLevel eventTypes)
    {
      return true;
    }

    public override void Debug(string message, object[] parameters = null, Exception exception = null)
    {
      writer.Write(new LogEventInfo(Name, LogLevel.Debug, message, parameters, exception));
    }

    public override void Info(string message, object[] parameters = null, Exception exception = null)
    {
      writer.Write(new LogEventInfo(Name, LogLevel.Info, message, parameters, exception));
    }

    public override void Warning(string message, object[] parameters = null, Exception exception = null)
    {
      writer.Write(new LogEventInfo(Name, LogLevel.Warning, message, parameters, exception));
    }

    public override void Error(string message, object[] parameters = null, Exception exception = null)
    {
      writer.Write(new LogEventInfo(Name, LogLevel.Error, message, parameters, exception));
    }

    public override void FatalError(string message, object[] parameters = null, Exception exception = null)
    {
      writer.Write(new LogEventInfo(Name, LogLevel.FatalError, message, parameters, exception));
    }

    public InternalLog(string name, ILogWriter writer)
      : base(name)
    {
      ArgumentValidator.EnsureArgumentNotNull(writer, "writer");
      this.writer = writer;
    }
  }
}
