// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that declares select operator over the <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public sealed class SelectProvider : UnaryProvider
  {
    private RecordHeader header;
    private readonly int[] columnToSelect;

    /// <summary>
    /// Indexes of columns that should be selected from the <see cref="UnaryProvider.Source"/>.
    /// </summary>
    public int[] ColumnsToSelect
    {
      get
      {
        var result = new int[columnToSelect.Length];
        columnToSelect.CopyTo(result, 0);
        return result;
      }
    }

    protected override RecordHeader BuildHeader()
    {
      return header;
    }

    protected override void Initialize()
    {
      IEnumerable<RecordColumn> columns = columnToSelect.Select(i => Source.Header.RecordColumnCollection[i]);
      TupleDescriptor tupleDescriptor = TupleDescriptor.Create(columnToSelect.Select(i => Source.Header.TupleDescriptor[i]));
      TupleDescriptor keyDescriptor = TupleDescriptor.Create(columnToSelect.Select(i => Source.Header.TupleDescriptor[i]));
      var orderBy = new DirectionCollection<int>();
      var keyIndicesByColumn =
        Source.Header.Keys.SelectMany(
          (keyInfo, i) => keyInfo.KeyColumns.Select(column => new KeyValuePair<RecordColumn, int>(column, i))).
          ToDictionary(pair => pair.Key, pair => pair.Value);

      var excludedKeys =
        Source.Header.RecordColumnCollection.Except(columns)
          .Select(column => Source.Header.Keys[keyIndicesByColumn[column]]);

      for (int i = 0; i < columnToSelect.Length; i++)
      {
        int columnIndex = columnToSelect[i];
        Direction direction;
        if (Source.Header.OrderInfo.OrderedBy.TryGetValue(columnIndex, out direction))
          orderBy.Add(i, direction);
      }

      header = new RecordHeader(tupleDescriptor, columns, keyDescriptor, Source.Header.Keys.Except(excludedKeys), orderBy); 
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SelectProvider(CompilableProvider provider, int[] columnIndexes)
      : base(provider)
    {
      columnToSelect = columnIndexes;
      Initialize();
    }
  }
}