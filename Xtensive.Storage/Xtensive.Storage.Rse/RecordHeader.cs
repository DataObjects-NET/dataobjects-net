// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.13

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using System.Linq;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Header of a RSE record.
  /// </summary>
  [Serializable]
  public sealed class RecordHeader
  {
    /// <summary>
    /// Gets or sets the record columns.
    /// </summary>
    public RecordColumnCollection RecordColumnCollection { get; private set; }

    /// <summary>
    /// Gets or sets the tuple descriptor.
    /// </summary>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <summary>
    /// Gets or sets the order info.
    /// </summary>
    public RecordOrderInfo OrderInfo { get; private set; }

    /// <summary>
    /// Gets or sets the record's keys.
    /// </summary>
    /// <value>The keys.</value>
    public CollectionBaseSlim<KeyInfo> Keys { get; private set; }

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
          indexInfo.Columns.Select((column, i) => new KeyValuePair<int, Direction>(i,
            columnSortDirections.ContainsKey(column) ? columnSortDirections[column] : Direction.Positive)));
    }

    private static KeyValuePair<IEnumerable<KeyInfo>, IEnumerable<RecordColumn>> BuildRecordColumns(IndexInfo indexInfo)
    {
      var columnInfoIndices = new Dictionary<ColumnInfo, int>();
      int i = 0;
      var recordColumns = new RecordColumn[indexInfo.Columns.Count];
      foreach (var columnInfo in indexInfo.Columns) {
        recordColumns[i] = new RecordColumn(columnInfo, i, columnInfo.ValueType,
          (columnInfo.Attributes & ColumnAttributes.PrimaryKey) > 0
            ? ColumnKind.PartOfKey
            : ColumnKind.RelatedToKey);
        columnInfoIndices.Add(columnInfo, i++);
      }

      return new KeyValuePair<IEnumerable<KeyInfo>, IEnumerable<RecordColumn>>(
        new[] {new KeyInfo(indexInfo.KeyColumns.Select(pair => recordColumns[columnInfoIndices[pair.Key]]))},
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

      Keys = keys==null
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
      Keys = header.Keys;

      OrderInfo = new RecordOrderInfo(
        header.OrderInfo.OrderedBy,
        header.OrderInfo.KeyDescriptor);
    }

    public RecordHeader(RecordHeader header, IEnumerable<string> includedColumns)
    {
      IList<RecordColumn> list = new List<RecordColumn>();
      var keyIndicesByColumn =
        header.Keys.SelectMany(
          (keyInfo, i) => keyInfo.KeyColumns.Select(column => new KeyValuePair<RecordColumn, int>(column, i))).
          ToDictionary(pair => pair.Key, pair => pair.Value);
     
      foreach (string includedColumn in includedColumns) {
        RecordColumn recordColumn = header.RecordColumnCollection[includedColumn];
        list.Add(recordColumn);
      }

      var excludedKeys =
        header.RecordColumnCollection.Except(list).Select(column => header.Keys[keyIndicesByColumn[column]]);

      RecordColumnCollection = new RecordColumnCollection(list);
      TupleDescriptor = header.TupleDescriptor;
      Keys = new CollectionBaseSlim<KeyInfo>(header.Keys.Except(excludedKeys));

      OrderInfo = new RecordOrderInfo(
        header.OrderInfo.OrderedBy,
        header.OrderInfo.KeyDescriptor);
    }
  }
}