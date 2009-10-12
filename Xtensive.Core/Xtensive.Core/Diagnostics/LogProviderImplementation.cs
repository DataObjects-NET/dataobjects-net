// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;
using System.Diagnostics;

namespace Xtensive.Core.Diagnostics
{
  /// <summary>
  /// Default <see cref="ILogProvider"/> implementation.
  /// </summary>
  [Serializable]
  public sealed class LogProviderImplementation : LogProviderImplementationBase
  {
    /// <inheritdoc/>
    protected override ILog GetLogImplementation(IRealLog realLog)
    {
      return new LogImplementation(realLog);
    }

    /// <inheritdoc/>
    protected override IRealLog GetRealLog(string key)
    {
      if (key == LogProvider.Console)
        return new ConsoleLog(key);
      if (key == LogProvider.Null)
        return new NullLog(key);

#if DEBUG
      return new DebugLog(key);
#else
      return Debugger.IsAttached ? (IRealLog) new DebugLog(key) : new NullLog(key);
#endif
    }
  }
}