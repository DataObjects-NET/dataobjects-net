// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.IoC;
using Xtensive.Orm.Internals;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Base <see cref="Session"/> handler class.
  /// </summary>
  public abstract partial class SessionHandler : IDisposable, IAsyncDisposable
  {
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
      while (handler is ChainingSessionHandler chainingSessionHandler) {
        handler = chainingSessionHandler.ChainedHandler;
      }
      return handler;
    }

    /// <summary>
    /// Executes the specified query tasks.
    /// </summary>
    /// <param name="queryTasks">The query tasks to execute.</param>
    /// <param name="allowPartialExecution">if set to <see langword="true"/> partial execution is allowed.</param>
    public abstract void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution);

    /// <summary>
    /// Asynchronously executes the specified query tasks.
    /// Default implementation executes query tasks synchronously.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="queryTasks">The query tasks to execute.</param>
    /// <param name="allowPartialExecution">if set to <see langword="true"/> partial execution is allowed.</param>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task performing operation.</returns>
    public virtual Task ExecuteQueryTasksAsync(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution, CancellationToken token)
    {
      ExecuteQueryTasks(queryTasks, allowPartialExecution);
      return Task.CompletedTask;
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
    /// Asynchronously persists changed entities.
    /// Default implementation works synchronously.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="registry">The registry.</param>
    /// <param name="allowPartialExecution">if set to <see langword="true"/> dirty flush is allowed.</param>
    /// <param name="token">The token to cancel this operation</param>
    public virtual Task PersistAsync(EntityChangeRegistry registry, bool allowPartialExecution, CancellationToken token)
    {
      Persist(registry, allowPartialExecution);
      return Task.CompletedTask;
    }

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

    /// <inheritdoc/>
    public virtual ValueTask DisposeAsync() => default;

    internal abstract void SetStorageNode(StorageNode node);

    protected SessionHandler(Session session)
    {
      Session = session;
      Handlers = session.Handlers;
    }
  }
}