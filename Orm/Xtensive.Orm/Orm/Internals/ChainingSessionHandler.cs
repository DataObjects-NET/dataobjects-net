// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.06

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
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

    /// <inheritdoc/>
    public override bool TransactionIsStarted { get { return ChainedHandler.TransactionIsStarted; } }

    public override void SetCommandTimeout(int? commandTimeout)
    {
      ChainedHandler.SetCommandTimeout(commandTimeout);
    }

    /// <inheritdoc/>
    public override void BeginTransaction(Transaction transaction)
    {
      ChainedHandler.BeginTransaction(transaction);
    }

    /// <inheritdoc/>
    public override void CompletingTransaction(Transaction transaction)
    {
      ChainedHandler.CompletingTransaction(transaction);
    }

    /// <inheritdoc/>
    public override void CommitTransaction(Transaction transaction)
    {
      ChainedHandler.CommitTransaction(transaction);
    }

    /// <inheritdoc/>
    public override void RollbackTransaction(Transaction transaction)
    {
      ChainedHandler.RollbackTransaction(transaction);
    }

    /// <inheritdoc/>
    public override void CreateSavepoint(Transaction transaction)
    {
      ChainedHandler.CreateSavepoint(transaction);
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(Transaction transaction)
    {
      ChainedHandler.RollbackToSavepoint(transaction);
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(Transaction transaction)
    {
      ChainedHandler.ReleaseSavepoint(transaction);
    }

    /// <inheritdoc/>
    public override void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution)
    {
      ChainedHandler.ExecuteQueryTasks(queryTasks, allowPartialExecution);
    }

    /// <inheritdoc/>
    public override void Persist(EntityChangeRegistry registry, bool allowPartialExecution)
    {
      ChainedHandler.Persist(registry, allowPartialExecution);
    }

    public override StrongReferenceContainer Prefetch(Key key, TypeInfo type, IList<PrefetchFieldDescriptor> descriptors)
    {
      return ChainedHandler.Prefetch(key, type, descriptors);
    }

    /// <inheritdoc/>
    public override StrongReferenceContainer ExecutePrefetchTasks(bool skipPersist)
    {
      return ChainedHandler.ExecutePrefetchTasks(skipPersist);
    }

    /// <inheritdoc/>
    public override EntityState FetchEntityState(Key key)
    {
      return ChainedHandler.FetchEntityState(key);
    }

    /// <inheritdoc/>
    public override void FetchField(Key key, Model.FieldInfo field)
    {
      ChainedHandler.FetchField(key, field);
    }

    /// <inheritdoc/>
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


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="chainedHandler">The handler to be chained.</param>
    protected ChainingSessionHandler(SessionHandler chainedHandler)
    {
      ArgumentValidator.EnsureArgumentNotNull(chainedHandler, "chainedHandler");
      this.ChainedHandler = chainedHandler;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
      ChainedHandler.Dispose();
    }
  }
}