using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Diagnostics
{
  /// <summary>
  /// Base type for log providers.
  /// </summary>
  public abstract class LogProviderBase : ISynchronizable, ILogProvider
  {
    private readonly object syncRoot = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private readonly Dictionary<string, ILog> logs = new Dictionary<string, ILog>();

    /// <summary>
    /// Gets <see cref="ILog"/> object forwarding logging messages to console.
    /// </summary>
    public ILog ConsoleLog
    {
      get { return GetLog("Console"); }
    }

    /// <summary>
    /// Gets <see cref="ILog"/> object forwarding logging messages to nothing.
    /// </summary>
    public ILog NullLog
    {
      get { return GetLog("Null"); }
    }

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
    protected virtual ILog CreateLog(string key)
    {
      return GetLog(GetRealLog(key));
    }

    /// <summary>
    /// Gets the <see cref="ILog"/> instance.
    /// </summary>
    /// <param name="realLog">The real log.</param>
    /// <returns></returns>
    protected abstract ILog GetLog(IRealLog realLog);

    /// <summary>
    /// Gets the <see cref="IRealLog"/> instance.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    protected abstract IRealLog GetRealLog(string key);
  }
}