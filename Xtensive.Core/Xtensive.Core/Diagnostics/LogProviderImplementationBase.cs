// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Diagnostics
{
  /// <summary>
  /// Base type for log providers.
  /// </summary>
  public abstract class LogProviderImplementationBase : ISynchronizable, ILogProvider
  {
    private readonly object syncRoot = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private readonly Dictionary<string, ILog> logs = new Dictionary<string, ILog>();

    /// <inheritdoc/>
    public bool IsSynchronized
    {
      [DebuggerStepThrough]
      get { return true; }
    }

    /// <inheritdoc/>
    public object SyncRoot
    {
      [DebuggerStepThrough]
      get { return syncRoot; }
    }

    /// <summary>
    /// Gets the <see cref="ILog"/> object by its <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to get the log for.</param>
    /// <returns>The <see cref="ILog"/> object.</returns>
    public ILog GetLog(string key)
    {
      using (Locker.ReadRegion((object) SyncRoot)) {
        ILog log;
        if (!logs.TryGetValue(key, out log)) {
          log = CreateLog(key);
          logs[key] = log;
        }
        return log;
      }
    }

    /// <summary>
    /// Creates the log.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns><see cref="ILog"/> instance.</returns>
    private ILog CreateLog(string key)
    {
      return GetLogImplementation(GetRealLog(key));
    }

    /// <summary>
    /// Gets the <see cref="ILog"/> instance.
    /// </summary>
    /// <param name="realLog">The real log.</param>
    /// <returns></returns>
    protected abstract ILog GetLogImplementation(IRealLog realLog);

    /// <summary>
    /// Gets the <see cref="IRealLog"/> instance.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    protected abstract IRealLog GetRealLog(string key);
  }
}