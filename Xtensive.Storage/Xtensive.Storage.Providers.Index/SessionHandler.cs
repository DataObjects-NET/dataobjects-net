// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.01

using System;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Index
{
  public class SessionHandler : Providers.SessionHandler
  {
    /// <inheritdoc/>
    public override void BeginTransaction()
    {
      // TODO: Implement transactions;
    }

    /// <inheritdoc/>
    public override void CommitTransaction()
    {
      // TODO: Implement transactions;
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
      // TODO: Implement transactions;
    }

    protected override void Insert(EntityData data)
    {
      var handler = (DomainHandler)Handlers.DomainHandler;
      foreach (IndexInfo indexInfo in data.Type.AffectedIndexes) {
        var index = handler.GetRealIndex(indexInfo);
        var transform = handler.GetIndexTransform(indexInfo, data.Type);
        index.Add(transform.Apply(TupleTransformType.Tuple, data));
      }
    }

    protected override void Update(EntityData data)
    {
      Remove(data);
      Insert(data);
    }

    protected override void Remove(EntityData data)
    {
      var handler = (DomainHandler)Handlers.DomainHandler;
      IndexInfo primaryIndex = data.Type.Indexes.PrimaryIndex;
      var indexProvider = IndexProvider.Get(primaryIndex);
      SeekResult<Tuple> result = indexProvider.GetService<IOrderedEnumerable<Tuple, Tuple>>().Seek(new Ray<IEntire<Tuple>>(Entire<Tuple>.Create(data.Key.Tuple)));

      if (result.ResultType!=SeekResultType.Exact)
        throw new InvalidOperationException();

      foreach (IndexInfo indexInfo in data.Type.AffectedIndexes) {
        var index = handler.GetRealIndex(indexInfo);
        var transform = handler.GetIndexTransform(indexInfo, data.Type);
        index.Remove(transform.Apply(TupleTransformType.TransformedTuple, result.Result));
      }
    }

    public override void Initialize()
    {
      // TODO: Think what should be done here.
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
    }
  }
}
