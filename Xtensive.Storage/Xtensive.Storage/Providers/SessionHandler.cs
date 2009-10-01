// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Base <see cref="Session"/> handler class.
  /// </summary>
  public abstract partial class SessionHandler : InitializableHandlerBase,
    IDisposable
  {
    private PrefetchProcessor prefetchProcessor;

    /// <summary>
    /// The <see cref="object"/> to synchronize access to a connection.
    /// </summary>
    protected readonly object ConnectionSyncRoot = new object();

    /// <summary>
    /// Determines whether an auto-shortened transaction is activated.
    /// </summary>
    protected bool IsAutoshortenTransactionActivated;

    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public Session Session { get; internal set; }

    ///<summary>
    /// Gets the specified <see cref="IsolationLevel"/>.
    ///</summary>
    public IsolationLevel DefaultIsolationLevel { get; internal set; }

    /// <summary>
    /// Gets the query provider.
    /// </summary>
    public virtual QueryProvider Provider {get { return QueryProvider.Instance; }}

    internal virtual int PrefetchTaskExecutionCount { get { return prefetchProcessor.TaskExecutionCount;} }

    /// <summary>
    /// Opens the transaction.
    /// </summary>
    public abstract void BeginTransaction();

    /// <summary>
    /// Commits the transaction.
    /// </summary>    
    public virtual void CommitTransaction()
    {
      prefetchProcessor.Clear();
    }

    /// <summary>
    /// Rollbacks the transaction.
    /// </summary>    
    public virtual void RollbackTransaction()
    {
      prefetchProcessor.Clear();
    }

    /// <summary>
    /// Acquires the connection lock.
    /// </summary>
    /// <returns>An implementation of <see cref="IDisposable"/> which should be disposed 
    /// to release the connection lock.</returns>
    public IDisposable AcquireConnectionLock()
    {
      Monitor.Enter(ConnectionSyncRoot);
      return new Disposable<object>(ConnectionSyncRoot, (disposing, syncRoot) => Monitor.Exit(syncRoot));
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      prefetchProcessor = new PrefetchProcessor(this);

      PersistRequiresTopologicalSort =
        (Handlers.Domain.Configuration.ForeignKeyMode & ForeignKeyMode.Reference) > 0 &&
         Handlers.Domain.Handler.ProviderInfo.Supports(ProviderFeatures.ForeignKeyConstraints) &&
        !Handlers.Domain.Handler.ProviderInfo.Supports(ProviderFeatures.DeferrableConstraints);
    }

    /// <inheritdoc/>
    public abstract void Dispose();
    
    /// <summary>
    /// Executes the specified compiled RSE query.
    /// This method is used only for non-index storages.
    /// </summary>
    /// <param name="provider">The provider to execute.</param>
    /// <returns>Result of query execution.</returns>
    public virtual IEnumerator<Tuple> Execute(ExecutableProvider provider)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Executes the specified query tasks.
    /// </summary>
    /// <param name="queryTasks">The query tasks to execute.</param>
    /// <param name="dirty">if set to <see langword="true"/> dirty execution is allowed.</param>
    public virtual void Execute(IList<QueryTask> queryTasks, bool dirty)
    {
      foreach (var task in queryTasks) {
        using (task.ParameterContext.ActivateSafely())
        using (EnumerationScope.Open())
          task.Result = task.DataSource.ToList();
      }
    }

    /// <summary>
    /// Register the task prefetching fields' values of the <see cref="Entity"/> with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="type">The type of the <see cref="Entity"/>.</param>
    /// <param name="descriptors">The descriptors of fields which values will be loaded.</param>
    public virtual void Prefetch(Key key, TypeInfo type, params PrefetchFieldDescriptor[] descriptors)
    {
      prefetchProcessor.Prefetch(key, type, descriptors);
    }

    /// <summary>
    /// Executes registered prefetch tasks.
    /// </summary>
    public virtual void ExecutePrefetchTasks()
    {
      prefetchProcessor.ExecuteTasks();
    }

    /// <summary>
    /// Updates the state of the <see cref="EntitySet{TItem}"/>.
    /// </summary>
    /// <param name="key">The owner's key.</param>
    /// <param name="fieldInfo">The referencing field.</param>
    /// <param name="items">The items.</param>
    /// <returns>The updated <see cref="EntitySetState"/>, or <see langword="null"/> 
    /// if a state was not found.</returns>
    protected EntitySetState UpdateEntitySetState(Key key, FieldInfo fieldInfo, IEnumerable<Key> items)
    {
      var entityState = Session.EntityStateCache[key, true];
      if (entityState==null)
        return null;
      var entity = entityState.Entity;
      if (entity==null)
        return null;
      var entitySet = Session.CoreServices.EntitySetAccessor.GetEntitySet(entity, fieldInfo);
      return entitySet.UpdateState(items);
    }

    /// <summary>
    /// Fetches an <see cref="EntityState"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The key of fetched <see cref="EntityState"/>.</returns>
    protected internal virtual EntityState FetchInstance(Key key)
    {
      var type = key.IsTypeCached ? key.Type : key.Hierarchy.Root;
      prefetchProcessor.Prefetch(key, type, type.Fields.Where(PrefetchTask.IsFieldIntrinsicNonLazy)
        .Select(field => new PrefetchFieldDescriptor(field)).ToArray());
      prefetchProcessor.ExecuteTasks();
      EntityState result;
      return TryGetEntityState(key, out result) ? result : null;
    }

    /// <summary>
    /// Fetches the field of an <see cref="Entity"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="field">The field to fetch.</param>
    protected internal virtual void FetchField(Key key, FieldInfo field)
    {
      var type = key.IsTypeCached ? key.Type : key.Hierarchy.Root;
      prefetchProcessor.Prefetch(key, type, new PrefetchFieldDescriptor(field));
      prefetchProcessor.ExecuteTasks();
    }

    /// <summary>
    /// Determines whether autoshorten transactions is enabled.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if autoshorten transactions is enabled; otherwise, <see langword="false"/>.
    /// </returns>
    protected bool IsAutoshortenTransactionsEnabled()
    {
      return (Session.Configuration.Options & SessionOptions.AutoShortenTransactions)
        ==SessionOptions.AutoShortenTransactions;
    }

    internal virtual EntityState RegisterEntityState(Key key, Tuple tuple)
    {
      return Session.UpdateEntityState(key, tuple);
    }

    internal virtual bool TryGetEntityState(Key key, out EntityState entityState)
    {
      return Session.EntityStateCache.TryGetItem(key, true, out entityState);
    }

    internal virtual EntitySetState RegisterEntitySetState(Key key, FieldInfo fieldInfo, bool isFullyLoaded, 
      List<Pair<Key, Tuple>> entities, List<Pair<Key, Tuple>> auxEntities)
    {
      if (Session.EntityStateCache[key, false]==null)
        return null;
      var keyList = new List<Key>();
      foreach (var pair in entities) {
        RegisterEntityState(pair.First, pair.Second);
        keyList.Add(pair.First);
      }
      return UpdateEntitySetState(key, fieldInfo, keyList);
    }

    internal virtual bool TryGetEntitySetState(Key key, FieldInfo fieldInfo, out EntitySetState entitySetState)
    {
      var entityState = Session.EntityStateCache[key, false];
      if (entityState!=null) {
        var entity = entityState.Entity;
        if (entity!=null) {
          var entitySet = Session.CoreServices.EntitySetAccessor.GetEntitySet(entity, fieldInfo);
          entitySetState = entitySet.GetState();
          return entitySetState!=null;
        }
      }
      entitySetState = null;
      return false;
    }

    internal void ChangeOwnerOfPrefetchProccessor(SessionHandler newOwner)
    {
      prefetchProcessor.ChangeOwner(newOwner);
    }
  }
}