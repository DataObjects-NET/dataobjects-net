// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Index.Resources;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Indexing;

namespace Xtensive.Storage.Providers.Index
{
  public class SessionHandler : Providers.SessionHandler
  {
    private readonly Dictionary<CompilableProvider, ExecutableProvider> compiledIndexProvidersCache 
      = new Dictionary<CompilableProvider, ExecutableProvider>();

    private IndexStorage storage;
    
    /// <summary>
    /// Gets the storage view.
    /// </summary>
    public IStorageView StorageView { get; private set; }

    /// <inheritdoc/>
    public override void BeginTransaction()
    {
       if (StorageView!=null)
        throw new InvalidOperationException();
       StorageView = storage.CreateView(Session.Transaction.IsolationLevel);
      // TODO: Implement transactions;
    }

    /// <inheritdoc/>
    public override void CommitTransaction()
    {
       if (StorageView == null)
        throw new InvalidOperationException();
       StorageView.Transaction.Commit();
       StorageView = null;
      // TODO: Implement transactions;
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
       if (StorageView == null)
        throw new InvalidOperationException();
       StorageView.Transaction.Rollback();
       StorageView = null;
      // TODO: Implement transactions;
    }

    /// <inheritdoc/>
    protected override void Insert(EntityState state)
    {
      var handler = (DomainHandler) Handlers.DomainHandler;
      foreach (IndexInfo indexInfo in state.Type.AffectedIndexes) {
        var index = handler.GetRealIndex(indexInfo);
        var transform = handler.GetIndexTransform(indexInfo, state.Type);
        var item = transform.Apply(TupleTransformType.Tuple, state.Tuple);
        index.Add(item);
      }
    }

    /// <inheritdoc/>
    protected override void Update(EntityState state)
    {
      var handler = (DomainHandler)Handlers.DomainHandler;
      IndexInfo primaryIndex = state.Type.Indexes.PrimaryIndex;
      var indexProvider = Rse.Providers.Compilable.IndexProvider.Get(primaryIndex);
      SeekResult<Tuple> pkSeekResult;
      using (EnumerationScope.Open()) {
        var executableProvider = GetCompiledIndexProvider(indexProvider);
        pkSeekResult = executableProvider
          .GetService<IOrderedEnumerable<Tuple, Tuple>>()
          .Seek(new Ray<Entire<Tuple>>(new Entire<Tuple>(state.Key.Value)));
        if (pkSeekResult.ResultType != SeekResultType.Exact)
          throw new InvalidOperationException(string.Format(Strings.ExInstanceXIsNotFound, 
            state.Key.EntityType.Name));
      }

      var pkItem = Tuple.Create(pkSeekResult.Result.Descriptor);
      pkSeekResult.Result.CopyTo(pkItem);
      pkItem.MergeWith(state.Tuple, MergeBehavior.PreferDifference);

      foreach (IndexInfo indexInfo in state.Type.AffectedIndexes) {
        var index = handler.GetRealIndex(indexInfo);
        var transform = handler.GetIndexTransform(indexInfo, state.Type);
        var oldItem = transform.Apply(TupleTransformType.TransformedTuple, pkSeekResult.Result).ToFastReadOnly();
        var item    = transform.Apply(TupleTransformType.Tuple, pkItem);
        index.Remove(oldItem);
        index.Add(item);
      }
    }

    /// <inheritdoc/>
    protected override void Remove(EntityState state)
    {
      var handler = (DomainHandler)Handlers.DomainHandler;
      IndexInfo primaryIndex = state.Type.Indexes.PrimaryIndex;
      var indexProvider = Rse.Providers.Compilable.IndexProvider.Get(primaryIndex);
      SeekResult<Tuple> pkSeekResult;
      using (EnumerationScope.Open()) {
        var executableProvider = GetCompiledIndexProvider(indexProvider);
        pkSeekResult = executableProvider
          .GetService<IOrderedEnumerable<Tuple, Tuple>>()
          .Seek(new Ray<Entire<Tuple>>(new Entire<Tuple>(state.Key.Value)));
        if (pkSeekResult.ResultType!=SeekResultType.Exact)
          throw new InvalidOperationException(string.Format(Strings.ExInstanceXIsNotFound, 
            state.Key.EntityType.Name));
      }

      foreach (IndexInfo indexInfo in state.Type.AffectedIndexes) {
        var index = handler.GetRealIndex(indexInfo);
        var transform = handler.GetIndexTransform(indexInfo, state.Type);
        var oldItem = transform.Apply(TupleTransformType.TransformedTuple, pkSeekResult.Result).ToFastReadOnly();
        index.Remove(oldItem);
      }
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      storage = ((DomainHandler) Handlers.DomainHandler).GetIndexStorage();
      // TODO: Think what should be done here.
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
    }

    private ExecutableProvider GetCompiledIndexProvider(CompilableProvider provider)
    {
      ExecutableProvider result;
      if (compiledIndexProvidersCache.TryGetValue(provider, out result))
        return result;

      var compilationContext = Handlers.DomainHandler.CompilationContext;
      if (compilationContext == null)
        throw new InvalidOperationException();
      result = compilationContext.Compile(provider);
      compiledIndexProvidersCache.Add(provider, result);
      return result;
    }
  }
}
