// Copyright (C) 2013-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using System;

namespace Xtensive.Orm.Logging
{
  internal sealed class NullLog : BaseLog
  {
    public override bool IsLogged(LogLevel eventTypes)
    {
      return false;
    }

    public override void Debug(string message, object[] parameters = null, Exception exception = null)
    {
    }

    public override void Info(string message, object[] parameters = null, Exception exception = null)
    {
    }

    public override void Warning(string message, object[] parameters = null, Exception exception = null)
    {
    }

    public override void Error(string message, object[] parameters = null, Exception exception = null)
    {
    }

    public override void FatalError(string message, object[] parameters = null, Exception exception = null)
    {
    }

    public override void Write(in LogEventInfo info)
    {
    }

    public NullLog(string name)
      : base(name)
    {
    }
  }
}