// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using Xtensive.Core;

namespace Xtensive.Orm.Logging
{
  internal sealed class InternalLog : BaseLog
  {
    private readonly LogWriter writer;

    public override bool IsLogged(LogLevel eventTypes)
    {
      return true;
    }

    public override void Write(LogEventInfo info)
    {
      writer.Write(info);
    }

    public InternalLog(string name, LogWriter writer)
      : base(name)
    {
      ArgumentValidator.EnsureArgumentNotNull(writer, "writer");
      this.writer = writer;
    }
  }
}
