// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.06

using System.Collections.Generic;
using Xtensive.Core;
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
    public readonly SessionHandler ChainedHandler;

    /// <inheritdoc/>
    public override QueryProvider Provider { get { return ChainedHandler.Provider; } }

    internal override int PrefetchTaskExecutionCount {
      get { return ChainedHandler.PrefetchTaskExecutionCount; }
    }
    
    /// <inheritdoc/>
    public override void BeginTransaction()
    {
      ChainedHandler.BeginTransaction();
    }

    /// <inheritdoc/>
    public override void CommitTransaction()
    {
      ChainedHandler.CommitTransaction();
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
      ChainedHandler.RollbackTransaction();
    }

    /// <inheritdoc/>
    public override void ExecuteQueryTasks(IList<QueryTask> queryTasks, bool allowPartialExecution)
    {
      ChainedHandler.ExecuteQueryTasks(queryTasks, allowPartialExecution);
    }

    /// <inheritdoc/>
    public override void Persist(EntityChangeRegistry registry, bool allowPartialExecution)
    {
      ChainedHandler.Persist(registry, allowPartialExecution);
    }

    /// <inheritdoc/>
    public override void Persist(IEnumerable<PersistAction> persistActions, bool allowPartialExecution)
    {
      ChainedHandler.Persist(persistActions, allowPartialExecution);
    }

    /// <inheritdoc/>
    public override StrongReferenceContainer Prefetch(Key key, Model.TypeInfo type,
      params PrefetchFieldDescriptor[] descriptors)
    {
      return ChainedHandler.Prefetch(key, type, descriptors);
    }

    /// <inheritdoc/>
    public override StrongReferenceContainer ExecutePrefetchTasks()
    {
      return ChainedHandler.ExecutePrefetchTasks();
    }
    
    protected internal override EntityState FetchInstance(Key key)
    {
      return ChainedHandler.FetchInstance(key);
    }

    protected internal override void FetchField(Key key, Model.FieldInfo field)
    {
      ChainedHandler.FetchField(key, field);
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

    public override T GetService<T>()
    {
      return ChainedHandler.GetService<T>();
    }

    public override void OnEnumerationContextCreated()
    {
      ChainedHandler.OnEnumerationContextCreated();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="chainedHandler">The handler to be chained.</param>
    protected ChainingSessionHandler(SessionHandler chainedHandler)
    {
      ArgumentValidator.EnsureArgumentNotNull(chainedHandler, "chainedHandler");
      ChainedHandler = chainedHandler;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
      ChainedHandler.Dispose();
    }
  }
}