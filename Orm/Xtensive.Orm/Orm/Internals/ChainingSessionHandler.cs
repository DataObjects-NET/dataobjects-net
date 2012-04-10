// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.06

using System;
using System.Collections.Generic;
using System.Data;
using Xtensive.Collections;
using Xtensive.Core;

using Xtensive.Orm.Model;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// The base class for <see cref="SessionHandler"/>s which support the chaining 
  /// with another handler.
  /// </summary>
  public abstract class ChainingSessionHandler : Providers.SessionHandler
  {
    /// <summary>
    /// The chained handler.
    /// </summary>
    protected internal readonly SessionHandler ChainedHandler;

    internal override int PrefetchTaskExecutionCount {
      get { return ChainedHandler.PrefetchTaskExecutionCount; }
    }


    /// <summary>
    /// Gets a value indicating whether [transaction is started].
    /// </summary>
    /// <value>
    /// 	<c>true</c> if [transaction is started]; otherwise, <c>false</c>.
    /// </value>
    public override bool TransactionIsStarted { get { return ChainedHandler.TransactionIsStarted; } }

    /// <summary>
    /// Sets command timeout for all <see cref="IDbCommand"/> created within current instance.
    /// </summary>
    /// <param name="commandTimeout">The command timeout.</param>
    public override void SetCommandTimeout(int? commandTimeout)
    {
      ChainedHandler.SetCommandTimeout(commandTimeout);
    }


    /// <summary>
    /// Begins the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    public override void BeginTransaction(Transaction transaction)
    {
      ChainedHandler.BeginTransaction(transaction);
    }


    /// <summary>
    /// Completings the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    public override void CompletingTransaction(Transaction transaction)
    {
      ChainedHandler.CompletingTransaction(transaction);
    }


    /// <summary>
    /// Commits the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    public override void CommitTransaction(Transaction transaction)
    {
      ChainedHandler.CommitTransaction(transaction);
    }


    /// <summary>
    /// Rollbacks the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    public override void RollbackTransaction(Transaction transaction)
    {
      ChainedHandler.RollbackTransaction(transaction);
    }


    /// <summary>
    /// Creates the savepoint.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    public override void CreateSavepoint(Transaction transaction)
    {
      ChainedHandler.CreateSavepoint(transaction);
    }


    /// <summary>
    /// Rollbacks to savepoint.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    public override void RollbackToSavepoint(Transaction transaction)
    {
      ChainedHandler.RollbackToSavepoint(transaction);
    }


    /// <summary>
    /// Releases the savepoint.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    public override void ReleaseSavepoint(Transaction transaction)
    {
      ChainedHandler.ReleaseSavepoint(transaction);
    }


    /// <summary>
    /// Executes the specified query tasks.
    /// </summary>
    /// <param name="queryTasks">The query tasks to execute.</param>
    /// <param name="allowPartialExecution">if set to <see langword="true"/> partial execution is allowed.</param>
    public override void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution)
    {
      ChainedHandler.ExecuteQueryTasks(queryTasks, allowPartialExecution);
    }


    /// <summary>
    /// Persists the specified registry.
    /// </summary>
    /// <param name="registry">The registry.</param>
    /// <param name="allowPartialExecution">if set to <c>true</c> [allow partial execution].</param>
    public override void Persist(EntityChangeRegistry registry, bool allowPartialExecution)
    {
      ChainedHandler.Persist(registry, allowPartialExecution);
    }


    /// <summary>
    /// Persists the specified persist actions.
    /// </summary>
    /// <param name="persistActions">The persist actions.</param>
    /// <param name="allowPartialExecution">if set to <c>true</c> [allow partial execution].</param>
    public override void Persist(IEnumerable<PersistAction> persistActions, bool allowPartialExecution)
    {
      ChainedHandler.Persist(persistActions, allowPartialExecution);
    }

    /// <summary>
    /// Prefetches the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="type">The type.</param>
    /// <param name="descriptors">The descriptors.</param>
    /// <returns></returns>
    public override StrongReferenceContainer Prefetch(Key key, TypeInfo type, IList<PrefetchFieldDescriptor> descriptors)
    {
      return ChainedHandler.Prefetch(key, type, descriptors);
    }


    /// <summary>
    /// Executes the prefetch tasks.
    /// </summary>
    /// <param name="skipPersist">if set to <c>true</c> [skip persist].</param>
    /// <returns></returns>
    public override StrongReferenceContainer ExecutePrefetchTasks(bool skipPersist)
    {
      return ChainedHandler.ExecutePrefetchTasks(skipPersist);
    }


    /// <summary>
    /// Fetches the state of the entity.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    public override EntityState FetchEntityState(Key key)
    {
      return ChainedHandler.FetchEntityState(key);
    }


    /// <summary>
    /// Fetches the field.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="field">The field.</param>
    public override void FetchField(Key key, Model.FieldInfo field)
    {
      ChainedHandler.FetchField(key, field);
    }


    /// <summary>
    /// Fetches the entity set.
    /// </summary>
    /// <param name="ownerKey">The owner key.</param>
    /// <param name="field">The field.</param>
    /// <param name="itemCountLimit">The item count limit.</param>
    public override void FetchEntitySet(Key ownerKey, Model.FieldInfo field, int? itemCountLimit)
    {
      ChainedHandler.FetchEntitySet(ownerKey, field, itemCountLimit);
    }

    internal override EntitySetState RegisterEntitySetState(Key key, Model.FieldInfo fieldInfo,
      bool isFullyLoaded, List<Key> entities, List<Pair<Key, Tuple>> auxEntities)
    {
      return ChainedHandler.RegisterEntitySetState(key, fieldInfo, isFullyLoaded, entities, auxEntities);
    }

    internal override EntityState RegisterEntityState(Key key, Tuple tuple)
    {
      return ChainedHandler.RegisterEntityState(key, tuple);
    }

    internal override bool TryGetEntitySetState(Key key, Model.FieldInfo fieldInfo,
      out EntitySetState entitySetState)
    {
      return ChainedHandler.TryGetEntitySetState(key, fieldInfo, out entitySetState);
    }

    internal override bool TryGetEntityState(Key key, out EntityState entityState)
    {
      return ChainedHandler.TryGetEntityState(key, out entityState);
    }


    /// <summary>
    /// Gets the service.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public override T GetService<T>()
    {
      return ChainedHandler.GetService<T>();
    }


    /// <summary>
    /// Creates enumeration context.
    /// </summary>
    /// <returns>
    /// Created context.
    /// </returns>
    public override Rse.Providers.EnumerationContext CreateEnumerationContext()
    {
      return ChainedHandler.CreateEnumerationContext();
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="chainedHandler">The handler to be chained.</param>
    protected ChainingSessionHandler(SessionHandler chainedHandler)
    {
      ArgumentValidator.EnsureArgumentNotNull(chainedHandler, "chainedHandler");
      this.ChainedHandler = chainedHandler;
    }


    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    public override void Dispose()
    {
      ChainedHandler.Dispose();
    }
  }
}