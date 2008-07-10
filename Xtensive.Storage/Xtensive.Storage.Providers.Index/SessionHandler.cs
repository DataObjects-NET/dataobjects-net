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
using System.Linq;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Declaration;

namespace Xtensive.Storage.Providers.Index
{
  public class SessionHandler : Providers.SessionHandler
  {
    protected override void Insert(EntityData data)
    {
      Index.DomainHandler handler = (Index.DomainHandler)Session.Domain.Handler;

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
      Index.DomainHandler handler = (Index.DomainHandler)Session.Domain.Handler;

      IndexInfo primaryIndex = data.Type.Indexes.PrimaryIndex;
      var indexProvider = new IndexProvider(primaryIndex);
      SeekResult<Tuple> result = indexProvider.GetService<IOrderedEnumerable<Tuple,Tuple>>().Seek(new Ray<IEntire<Tuple>>(Entire<Tuple>.Create(data.Key.Tuple)));

      if (result.ResultType != SeekResultType.Exact)
        throw new InvalidOperationException();

      foreach (IndexInfo indexInfo in data.Type.AffectedIndexes) {
        var index = handler.GetRealIndex(indexInfo);
        var transform = handler.GetIndexTransform(indexInfo);
        index.Remove(transform.Apply(TupleTransformType.TransformedTuple, result.Result));
      }
    }

    public override Tuple Fetch(Key key, IEnumerable<ColumnInfo> columns)
    {
      Index.DomainHandler handler = (Index.DomainHandler)Session.Domain.Handler;
      var index = new IndexProvider(key.Type.Indexes.PrimaryIndex);
      SeekResult<Tuple> seek = index.GetService<IOrderedEnumerable<Tuple, Tuple>>().Seek(new Ray<IEntire<Tuple>>(Entire<Tuple>.Create(key.Tuple)));
      if (seek.ResultType!=SeekResultType.Exact)
        return null;
      return seek.Result;
    }

    public override Tuple FetchKey(Key key)
    {
      IndexInfo primaryIndex = key.Hierarchy.Root.Indexes.PrimaryIndex;
      IndexInfo indexInfo = primaryIndex.IsVirtual && (primaryIndex.Attributes & IndexAttributes.Union) == 0
        ? primaryIndex.BaseIndexes[0]
        : primaryIndex;
      Index.DomainHandler handler = (Index.DomainHandler)Session.Domain.Handler;
      var index = new IndexProvider(indexInfo);
      SeekResult<Tuple> seek = index.GetService<IOrderedEnumerable<Tuple, Tuple>>().Seek(new Ray<IEntire<Tuple>>(Entire<Tuple>.Create(key.Tuple)));
      if (seek.ResultType != SeekResultType.Exact)
        throw new InvalidOperationException();

      return seek.Result;
    }

    public override IEnumerable<Tuple> Select(TypeInfo type, IEnumerable<ColumnInfo> columns)
    {
      return new IndexProvider(type.Indexes.PrimaryIndex).Result;
    }

    public override RecordSet QueryIndex(IndexInfo info)
    {
      return new IndexProvider(info).Result;
    }
  }
}