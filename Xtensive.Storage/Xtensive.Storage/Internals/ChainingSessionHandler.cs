// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.06

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals.Prefetch;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// The base class for <see cref="SessionHandler"/>s which support the chaining 
  /// with another handler.
  /// </summary>
  public abstract class ChainingSessionHandler : SessionHandler
  {
    /// <summary>
    /// The chained handler.
    /// </summary>
    protected readonly SessionHandler chainedHandler;

    /// <inheritdoc/>
    public override QueryProvider Provider { get { return chainedHandler.Provider; } }

    internal override int PrefetchTaskExecutionCount {
      get { return chainedHandler.PrefetchTaskExecutionCount; }
    }

    /// <inheritdoc/>
    public override bool TransactionIsStarted { get { return chainedHandler.TransactionIsStarted; } }

    /// <inheritdoc/>
    public override void BeginTransaction()
    {
      chainedHandler.BeginTransaction();
    }

    /// <inheritdoc/>
    public override void CommitTransaction()
    {
      chainedHandler.CommitTransaction();
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
      chainedHandler.RollbackTransaction();
    }

    /// <inheritdoc/>
    public override void ExecuteQueryTasks(IList<QueryTask> queryTasks, bool allowPartialExecution)
    {
      chainedHandler.ExecuteQueryTasks(queryTasks, allowPartialExecution);
    }

    /// <inheritdoc/>
    public override void Persist(EntityChangeRegistry registry, bool allowPartialExecution)
    {
      chainedHandler.Persist(registry, allowPartialExecution);
    }

    /// <inheritdoc/>
    public override void Persist(IEnumerable<PersistAction> persistActions, bool allowPartialExecution)
    {
      chainedHandler.Persist(persistActions, allowPartialExecution);
    }

    /// <inheritdoc/>
    public override StrongReferenceContainer Prefetch(Key key, Model.TypeInfo type,
      FieldDescriptorCollection descriptors)
    {
      return chainedHandler.Prefetch(key, type, descriptors);
    }

    /// <inheritdoc/>
    public override StrongReferenceContainer ExecutePrefetchTasks()
    {
      return chainedHandler.ExecutePrefetchTasks();
    }
    
    protected internal override EntityState FetchInstance(Key key)
    {
      return chainedHandler.FetchInstance(key);
    }

    protected internal override void FetchField(Key key, Model.FieldInfo field)
    {
      chainedHandler.FetchField(key, field);
    }

    internal override EntitySetState RegisterEntitySetState(Key key, Model.FieldInfo fieldInfo,
      bool isFullyLoaded, List<Key> entities, List<Pair<Key, Tuple>> auxEntities)
    {
      return chainedHandler.RegisterEntitySetState(key, fieldInfo, isFullyLoaded, entities, auxEntities);
    }

    internal override EntityState RegisterEntityState(Key key, Tuple tuple)
    {
      return chainedHandler.RegisterEntityState(key, tuple);
    }

    internal override bool TryGetEntitySetState(Key key, Model.FieldInfo fieldInfo,
      out EntitySetState entitySetState)
    {
      return chainedHandler.TryGetEntitySetState(key, fieldInfo, out entitySetState);
    }

    internal override bool TryGetEntityState(Key key, out EntityState entityState)
    {
      return chainedHandler.TryGetEntityState(key, out entityState);
    }

    public override T GetService<T>()
    {
      return chainedHandler.GetService<T>();
    }

    public override Rse.Providers.EnumerationContext CreateEnumerationContext()
    {
      return chainedHandler.CreateEnumerationContext();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="chainedHandler">The handler to be chained.</param>
    protected ChainingSessionHandler(SessionHandler chainedHandler)
    {
      ArgumentValidator.EnsureArgumentNotNull(chainedHandler, "chainedHandler");
      this.chainedHandler = chainedHandler;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
      chainedHandler.Dispose();
    }
  }
}