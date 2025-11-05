// Copyright (C) 2013-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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

    public override void Write(in LogEventInfo info)
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
