// Copyright (C) 2013-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2013.09.16

namespace Xtensive.Orm.Logging
{
  internal abstract class LogWriter
  {
    public abstract void Write(in LogEventInfo logEvent);
  }
}
