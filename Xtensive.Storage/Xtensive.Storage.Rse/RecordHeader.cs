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
    private CollectionBaseSlim<KeyInfo> keys;

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

    public CollectionBaseSlim<KeyInfo> Keys
    {
      get { return keys; }
      set {
        this.EnsureNotLocked();
        keys = value;
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

    private static DirectionCollection<int> BuildOrderByCollection(IndexInfo indexInfo)
    {
      if (indexInfo.IsPrimary)
        return new DirectionCollection<int>(
          indexInfo.KeyColumns.Select((pair, i) => new KeyValuePair<int, Direction>(i, pair.Value)));

      var columnSortDirections = indexInfo.KeyColumns.Union(
        indexInfo.ReflectedType.Indexes.PrimaryIndex.KeyColumns).
        ToDictionary(pair => pair.Key, pair => pair.Value);

      return
        new DirectionCollection<int>(
          indexInfo.Columns.Select((column, i) => new KeyValuePair<int, Direction>(
            i,
            columnSortDirections.ContainsKey(column) ? columnSortDirections[column] : Direction.Positive)));
    }

    private static KeyValuePair<IEnumerable<KeyInfo>, IEnumerable<RecordColumn>> BuildRecordColumns(IndexInfo indexInfo)
    {
      var columnInfoIndices = new Dictionary<ColumnInfo, int>();
      int i = 0;
      var recordColumns = new RecordColumn[indexInfo.Columns.Count];
      foreach(var columnInfo in indexInfo.Columns) {
        recordColumns[i] = new RecordColumn(columnInfo, i, columnInfo.ValueType,
                                            (columnInfo.Attributes & ColumnAttributes.PrimaryKey) > 0
                                              ? ColumnType.PartOfKey
                                              : ColumnType.RelatedToKey);
        columnInfoIndices.Add(columnInfo, i++);
      }

      return new KeyValuePair<IEnumerable<KeyInfo>, IEnumerable<RecordColumn>>(
        new [] {new KeyInfo(indexInfo.KeyColumns.Select(pair => recordColumns[columnInfoIndices[pair.Key]]))},
        recordColumns);
    }


    // Constructors

    public RecordHeader(IndexInfo indexInfo)
      : this(
            TupleDescriptor.Create(indexInfo.Columns.Select(columnInfo => columnInfo.ValueType)),
            TupleDescriptor.Create(
              indexInfo.KeyColumns.Select(columnInfo => columnInfo.Key.ValueType)),
            BuildRecordColumns(indexInfo), 
            BuildOrderByCollection(indexInfo))
    {
    }

    private RecordHeader(TupleDescriptor tupleDescriptor, TupleDescriptor keyDescriptor, KeyValuePair<IEnumerable<KeyInfo>, IEnumerable<RecordColumn>> recordColumns, DirectionCollection<int> orderedBy)
      : this(tupleDescriptor, recordColumns.Value, keyDescriptor, recordColumns.Key, orderedBy)
    {
    }

    public RecordHeader(TupleDescriptor tupleDescriptor, IEnumerable<RecordColumn> recordColumns, TupleDescriptor keyDescriptor, IEnumerable<KeyInfo> keys, DirectionCollection<int> orderedBy)
    {
      ArgumentValidator.EnsureArgumentNotNull(tupleDescriptor, "tupleDescriptor");
      ArgumentValidator.EnsureArgumentNotNull(recordColumns, "recordColumns");
      TupleDescriptor = tupleDescriptor;
      RecordColumnCollection = new RecordColumnCollection(recordColumns);

      Keys = keys == null
                    ? new CollectionBaseSlim<KeyInfo>()
                    : new CollectionBaseSlim<KeyInfo>(keys);

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