// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Declaration
{
  public class SelectProvider : CompilableProvider
  {
    private readonly int[] columnIndexes;

    public int[] ColumnIndexes
    {
      get
      {
        var result = new int[columnIndexes.Length];
        columnIndexes.CopyTo(result, 0);
        return result;
      }
    }

    public CompilableProvider Source { get; private set; }

    protected override RecordHeader BuildHeader()
    {
      IEnumerable<RecordColumn> columns = columnIndexes.Select(i => Source.Header.RecordColumnCollection[i]);
      TupleDescriptor tupleDescriptor = TupleDescriptor.Create(columnIndexes.Select(i => Source.Header.TupleDescriptor[i]));
      TupleDescriptor keyDescriptor = TupleDescriptor.Create(columnIndexes.Select(i => Source.Header.TupleDescriptor[i]));
      var orderBy = new DirectionCollection<int>();
      for (int i = 0; i < columnIndexes.Length; i++) {
        int columnIndex = columnIndexes[i];
        Direction direction;
        if (Source.Header.OrderInfo.OrderedBy.TryGetValue(columnIndex, out direction))
          orderBy.Add(i, direction);
      }
      return new RecordHeader(tupleDescriptor, columns, keyDescriptor, ArrayUtils<KeyInfo>.EmptyArray, orderBy);
    }


    // Constructor

    public SelectProvider(CompilableProvider provider, int[] columnIndexes)
      : base(provider)
    {
      this.columnIndexes = columnIndexes;
      Source = provider;
    }
  }
}