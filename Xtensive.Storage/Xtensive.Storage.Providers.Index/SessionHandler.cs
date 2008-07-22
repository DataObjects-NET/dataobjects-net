// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.01

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Index
{
  public class SessionHandler : Providers.SessionHandler
  {
    protected override void Insert(EntityData data)
    {
      var handler = (DomainHandler)HandlerAccessor.DomainHandler;
      foreach (IndexInfo indexInfo in data.Type.AffectedIndexes) {
        var index = handler.GetRealIndex(indexInfo);
        var transform = handler.GetIndexTransform(indexInfo);
        index.Add(transform.Apply(TupleTransformType.Tuple, data.Tuple));
      }
    }

    protected override void Update(EntityData data)
    {
      Remove(data);
      Insert(data);
    }

    protected override void Remove(EntityData data)
    {
      var handler = (DomainHandler)HandlerAccessor.DomainHandler;
      IndexInfo primaryIndex = data.Type.Indexes.PrimaryIndex;
      var indexProvider = new IndexProvider(primaryIndex);
      SeekResult<Tuple> result = indexProvider.GetService<IOrderedEnumerable<Tuple, Tuple>>().Seek(new Ray<IEntire<Tuple>>(Entire<Tuple>.Create(data.Key.Tuple)));

      if (result.ResultType!=SeekResultType.Exact)
        throw new InvalidOperationException();

      foreach (IndexInfo indexInfo in data.Type.AffectedIndexes) {
        var index = handler.GetRealIndex(indexInfo);
        var transform = handler.GetIndexTransform(indexInfo);
        index.Remove(transform.Apply(TupleTransformType.TransformedTuple, result.Result));
      }
    }

    public override Tuple Fetch(IndexInfo index, Key key, IEnumerable<ColumnInfo> columns)
    {
      var indexProvider = new IndexProvider(index);
      SeekResult<Tuple> seek = indexProvider.GetService<IOrderedEnumerable<Tuple, Tuple>>().Seek(new Ray<IEntire<Tuple>>(Entire<Tuple>.Create(key.Tuple)));
      if (seek.ResultType!=SeekResultType.Exact)
        return null;
      return seek.Result;
    }

    public override RecordSet Select(IndexInfo index)
    {
      return new IndexProvider(index).Result;
    }
  }
}
