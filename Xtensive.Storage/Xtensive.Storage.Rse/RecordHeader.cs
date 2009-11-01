// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.13

using System;
using System.Collections.Generic;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using System.Linq;

namespace Xtensive.Storage.Rse
{
  [Serializable]
  public sealed class RecordHeader : LockableBase
  {
    private RecordColumnCollection recordColumnCollection;
    private TupleDescriptor tupleDescriptor;
    private RecordOrderInfo orderInfo;

    public RecordColumnCollection RecordColumnCollection {
      get { return recordColumnCollection; }
      set {
        this.EnsureNotLocked();
        recordColumnCollection = value;
      }
    }

    public TupleDescriptor TupleDescriptor {
      get { return tupleDescriptor; }
      set {
        this.EnsureNotLocked();
        tupleDescriptor = value;
      }
    }

    public RecordOrderInfo OrderInfo {
      get { return orderInfo; }
      set {
        this.EnsureNotLocked();
        orderInfo = value;
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (!recursive)
        return;
      
      foreach (RecordColumn column in RecordColumnCollection) {
        column.Lock(recursive);
      }
    }


    // Constructors

    public RecordHeader(IndexInfo indexInfo, TupleDescriptor keyDescriptor, DirectionCollection<int> orderedBy)
      : this(indexInfo.KeyColumns.Keys, keyDescriptor, orderedBy)
    {
    }

    public RecordHeader(IEnumerable<ColumnInfo> columns, TupleDescriptor keyDescriptor, DirectionCollection<int> orderedBy)
    {
      ArgumentValidator.EnsureArgumentNotNull(columns, "columns");
      byte i = 0;
      TupleDescriptor = TupleDescriptor.Create(columns.Select(columnInfo => columnInfo.ValueType));
      RecordColumnCollection =
        new RecordColumnCollection(
          columns.Select(
            columnInfo =>
            new RecordColumn(columnInfo, i++, columnInfo.ValueType,
                             (columnInfo.Attributes & ColumnAttributes.PrimaryKey) > 0
                               ? ColumnType.PartOfKey
                               : ColumnType.RelatedToKey)));

      OrderInfo = new RecordOrderInfo(
        orderedBy ?? new DirectionCollection<int>(),
        keyDescriptor ?? TupleDescriptor.Empty);
    }

    public RecordHeader(TupleDescriptor tupleDescriptor, IEnumerable<RecordColumn> recordColumns, TupleDescriptor keyDescriptor, DirectionCollection<int> orderedBy)
    {
      ArgumentValidator.EnsureArgumentNotNull(tupleDescriptor, "tupleDescriptor");
      ArgumentValidator.EnsureArgumentNotNull(recordColumns, "recordColumns");
      this.TupleDescriptor = tupleDescriptor;
      RecordColumnCollection = new RecordColumnCollection(recordColumns);

      OrderInfo = new RecordOrderInfo(
        orderedBy ?? new DirectionCollection<int>(),
        keyDescriptor ?? TupleDescriptor.Empty);
    }

    public RecordHeader(RecordHeader left, RecordHeader right)
    {
      RecordColumnCollection = new RecordColumnCollection(
        left.RecordColumnCollection,
        right.RecordColumnCollection.Select(
          column => new RecordColumn(column, left.RecordColumnCollection.Count + column.Index)));
      TupleDescriptor = TupleDescriptor.Create(new[] {left.TupleDescriptor, right.TupleDescriptor}.SelectMany(descriptor => descriptor));

      OrderInfo = new RecordOrderInfo(
        new DirectionCollection<int>(
          left.OrderInfo.OrderedBy.Union(
            right.OrderInfo.OrderedBy.Select(
              pair => new KeyValuePair<int, Direction>(pair.Key + left.RecordColumnCollection.Count, pair.Value)))),
        TupleDescriptor.Create(new[] {left.OrderInfo.KeyDescriptor, right.OrderInfo.KeyDescriptor}.SelectMany(descriptor => descriptor)));
    }

    public RecordHeader(RecordHeader header, string alias)
    {
      RecordColumnCollection = new RecordColumnCollection(header.RecordColumnCollection, alias);
      TupleDescriptor = header.TupleDescriptor;

      OrderInfo = new RecordOrderInfo(
        header.OrderInfo.OrderedBy,
        header.OrderInfo.KeyDescriptor);
    }

    public RecordHeader(RecordHeader header, IEnumerable<string> includedColumns)
    {
      IList<RecordColumn> list = new List<RecordColumn>();
      foreach (string includedColumn in includedColumns) {
        RecordColumn recordColumn = header.RecordColumnCollection[includedColumn];
        list.Add(recordColumn);
      }
      RecordColumnCollection = new RecordColumnCollection(list);
      TupleDescriptor = header.TupleDescriptor;

      OrderInfo = new RecordOrderInfo(
        header.OrderInfo.OrderedBy,
        header.OrderInfo.KeyDescriptor);
    }
  }
}