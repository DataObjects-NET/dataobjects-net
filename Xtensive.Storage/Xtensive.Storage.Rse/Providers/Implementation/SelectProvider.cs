// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.20

using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Rse;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Storage.Rse.Providers.Implementation
{
  public sealed class SelectProvider : ProviderWrapper
  {
    private readonly RecordHeader header;

    public override IEnumerator<Tuple> GetEnumerator()
    {
      throw new System.NotImplementedException();
    }


    // Constructors

    public SelectProvider(RecordHeader header, Provider source, int[] columnIndexes)
      : base(header, source)
    {
      IEnumerable<RecordColumn> columns = columnIndexes.Select(i => source.Header.RecordColumnCollection[i]);
      TupleDescriptor tupleDescriptor = TupleDescriptor.Create(columnIndexes.Select(i => source.Header.TupleDescriptor[i]));
      TupleDescriptor keyDescriptor = TupleDescriptor.Create(columnIndexes.Select(i => source.Header.TupleDescriptor[i]));
      var orderBy = new DirectionCollection<int>();
      for (int i = 0; i < columnIndexes.Length; i++) {
        int columnIndex = columnIndexes[i];
        Direction direction;
        if (source.Header.OrderInfo.OrderedBy.TryGetValue(columnIndex, out direction))
          orderBy.Add(i, direction);
      }
      header = new RecordHeader(tupleDescriptor, columns, keyDescriptor, orderBy);
    }
  }
}