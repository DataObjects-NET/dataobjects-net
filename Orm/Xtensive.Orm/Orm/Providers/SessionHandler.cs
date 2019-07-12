// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Base <see cref="Session"/> handler class.
  /// </summary>
  public abstract partial class SessionHandler : IDisposable
  {
    private static readonly object CachingRegion = new object();

    /// <summary>
    /// Gets <see cref="HandlerAccessor"/>.
    /// </summary>
    protected HandlerAccessor Handlers { get; private set; }

    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public Session Session { get; private set; }

    /// <summary>
    /// Gets the real session handler (the final handler in chain of all <see cref="ChainingSessionHandler"/>s).
    /// </summary>
    /// <returns>The real session handler.</returns>
    public SessionHandler GetRealHandler()
    {
      var handler = this;
      while (handler is ChainingSessionHandler)
        handler = (handler as ChainingSessionHandler).ChainedHandler;
      return handler;
    }

    /// <summary>
    /// Executes the specified query tasks.
    /// </summary>
    /// <param name="queryTasks">The query tasks to execute.</param>
    /// <param name="allowPartialExecution">if set to <see langword="true"/> partial execution is allowed.</param>
    public abstract void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution);

    public virtual async Task ExecuteQueryTasksAsync(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution, CancellationToken token)
    {
      ExecuteQueryTasks(queryTasks, allowPartialExecution);
      await Task.Yield();
    }

    /// <summary>
    /// Sets command timeout for all <see cref="IDbCommand"/> created within current instance.
    /// </summary>
    /// <param name="commandTimeout">The command timeout.</param>
    public abstract void SetCommandTimeout(int? commandTimeout);

    /// <summary>
    /// Persists changed entities.
    /// </summary>
    /// <param name="registry">The registry.</param>
    /// <param name="allowPartialExecution">if set to <see langword="true"/> dirty flush is allowed.</param>
    public abstract void Persist(EntityChangeRegistry registry, bool allowPartialExecution);

    /// <summary>
    /// Adds system session service registration entries.
    /// </summary>
    /// <param name="r">The list of service registrations.</param>
    public virtual void AddSystemServices(ICollection<ServiceRegistration> r)
    {
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
    }

    internal abstract void SetStorageNode(StorageNode node);

    protected SessionHandler(Session session)
    {
      Session = session;
      Handlers = session.Handlers;
    }
  }
}