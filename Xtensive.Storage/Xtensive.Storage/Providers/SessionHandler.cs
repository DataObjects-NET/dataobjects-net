// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Internals.Prefetch;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Base <see cref="Session"/> handler class.
  /// </summary>
  public abstract partial class SessionHandler : InitializableHandlerBase,
    IDisposable, IHasServices
  {
    private static readonly object CachingRegion = new object();
    
    /// <summary>
    /// The <see cref="object"/> to synchronize access to a connection.
    /// </summary>
    protected readonly object connectionSyncRoot = new object();

    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public Session Session { get; internal set; }

    /// <summary>
    /// Gets the query provider.
    /// </summary>
    public virtual QueryProvider Provider {get { return QueryProvider.Instance; }}
    
    /// <summary>
    /// Acquires the connection lock.
    /// </summary>
    /// <returns>An implementation of <see cref="IDisposable"/> which should be disposed 
    /// to release the connection lock.</returns>
    public IDisposable AcquireConnectionLock()
    {
      Monitor.Enter(connectionSyncRoot);
      return new Disposable<object>(connectionSyncRoot, (disposing, syncRoot) => Monitor.Exit(syncRoot));
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      prefetchManager = new PrefetchManager(Session);

      persistRequiresTopologicalSort =
        (Handlers.Domain.Configuration.ForeignKeyMode & ForeignKeyMode.Reference) > 0 &&
         Handlers.Domain.Handler.ProviderInfo.Supports(ProviderFeatures.ForeignKeyConstraints) &&
        !Handlers.Domain.Handler.ProviderInfo.Supports(ProviderFeatures.DeferrableConstraints);
    }

    /// <inheritdoc/>
    public abstract void Dispose();

    /// <summary>
    /// Gets the key generator.
    /// </summary>
    /// <param name="keyProviderInfo">The key provider info.</param>
    public virtual KeyGenerator GetKeyGenerator(KeyProviderInfo keyProviderInfo)
    {
      return null;
    }

    /// <summary>
    /// Creates enumeration context.
    /// </summary>
    /// <returns>Created context.</returns>
    public virtual Rse.Providers.EnumerationContext CreateEnumerationContext()
    {
      return new EnumerationContext(GetEnumerationContextOptions());
    }

    /// <summary>
    /// Executes the specified query tasks.
    /// </summary>
    /// <param name="queryTasks">The query tasks to execute.</param>
    /// <param name="allowPartialExecution">if set to <see langword="true"/> partial execution is allowed.</param>
    public virtual void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution)
    {
      foreach (var task in queryTasks) {
        using (EnumerationScope.Open())
        using (task.ParameterContext.ActivateSafely())
          task.Result = task.DataSource.ToList();
      }
    }

    /// <summary>
    /// Gets the enumeration context options.
    /// </summary>
    /// <returns>Options for new enumeration context.</returns>
    protected virtual EnumerationContextOptions GetEnumerationContextOptions()
    {
      var options = EnumerationContextOptions.Default;
      if (!Handlers.DomainHandler.ProviderInfo.Supports(ProviderFeatures.MultipleActiveResultSets))
        options |= EnumerationContextOptions.PreloadEnumerator;
      return options;
    }

    #region IHasServices members

    public virtual T GetService<T>()
      where T : class
    {
      var result = this as T;
      if (result==null)
        throw new InvalidOperationException(
          string.Format(Strings.ExServiceXIsNotSupported, typeof (T).GetFullName()));
      return result;
    }

    #endregion
  }
}