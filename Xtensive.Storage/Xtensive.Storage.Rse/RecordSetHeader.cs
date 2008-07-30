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
  /// Header of <see cref="RecordSet"/>.
  /// </summary>
  [Serializable]
  public sealed class RecordSetHeader
  {
    /// <summary>
    /// Gets the <see cref="RecordSet"/> keys.
    /// </summary>
    /// <value>The keys.</value>
    public CollectionBaseSlim<KeyInfo> Keys { get; private set; }

    /// <summary>
    /// Gets the <see cref="RecordSet"/> columns.
    /// </summary>
    public RecordColumnCollection Columns { get; private set; }

    /// <summary>
    /// Gets the <see cref="RecordSet"/> tuple descriptor.
    /// </summary>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <summary>
    /// Gets the <see cref="RecordSet"/> order descriptor.
    /// </summary>
    public RecordSetOrderDescriptor OrderDescriptor { get; private set; }

    private static DirectionCollection<int> BuildOrderDescriptor(IndexInfo indexInfo)
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

    private static KeyValuePair<IEnumerable<KeyInfo>, IEnumerable<RecordColumn>> BuildColumns(IndexInfo indexInfo)
    {
      var columnInfoIndices = new Dictionary<ColumnInfo, int>();
      int i = 0;
      var recordColumns = new RecordColumn[indexInfo.Columns.Count];
      foreach (var columnInfo in indexInfo.Columns) {
        recordColumns[i] = new RecordColumn(columnInfo, i, columnInfo.ValueType,
          (columnInfo.Attributes & ColumnAttributes.PrimaryKey) > 0
            ? ColumnKind.Key
            : ColumnKind.KeyRelated);
        columnInfoIndices.Add(columnInfo, i++);
      }

      return new KeyValuePair<IEnumerable<KeyInfo>, IEnumerable<RecordColumn>>(
        new[] {new KeyInfo(indexInfo.KeyColumns.Select(pair => recordColumns[columnInfoIndices[pair.Key]]))},
        recordColumns);
    }


    // Constructors

    public RecordSetHeader(IndexInfo indexInfo)
      : this(
            TupleDescriptor.Create(indexInfo.Columns.Select(columnInfo => columnInfo.ValueType)),
            TupleDescriptor.Create(
              indexInfo.KeyColumns.Select(columnInfo => columnInfo.Key.ValueType)),
            BuildColumns(indexInfo), 
            BuildOrderDescriptor(indexInfo))
    {
    }

    private RecordSetHeader(TupleDescriptor tupleDescriptor, TupleDescriptor keyDescriptor, KeyValuePair<IEnumerable<KeyInfo>, IEnumerable<RecordColumn>> recordColumns, DirectionCollection<int> orderedBy)
      : this(tupleDescriptor, recordColumns.Value, keyDescriptor, recordColumns.Key, orderedBy)
    {
    }

    public RecordSetHeader(TupleDescriptor tupleDescriptor, IEnumerable<RecordColumn> recordColumns, TupleDescriptor keyDescriptor, IEnumerable<KeyInfo> keys, DirectionCollection<int> orderedBy)
    {
      ArgumentValidator.EnsureArgumentNotNull(tupleDescriptor, "tupleDescriptor");
      ArgumentValidator.EnsureArgumentNotNull(recordColumns, "recordColumns");
      TupleDescriptor = tupleDescriptor;
      Columns = new RecordColumnCollection(recordColumns);

      Keys = keys==null
        ? new CollectionBaseSlim<KeyInfo>()
        : new CollectionBaseSlim<KeyInfo>(keys);

      OrderDescriptor = new RecordSetOrderDescriptor(
        orderedBy ?? new DirectionCollection<int>(),
        keyDescriptor ?? TupleDescriptor.Empty);
    }

    public RecordSetHeader(RecordSetHeader left, RecordSetHeader right)
    {
      Columns = new RecordColumnCollection(
        left.Columns,
        right.Columns.Select(
          column => new RecordColumn(column, left.Columns.Count + column.Index)));
      TupleDescriptor = TupleDescriptor.Create(new[] {left.TupleDescriptor, right.TupleDescriptor}.SelectMany(descriptor => descriptor));

      OrderDescriptor = new RecordSetOrderDescriptor(
        new DirectionCollection<int>(
          left.OrderDescriptor.Order.Union(
            right.OrderDescriptor.Order.Select(
              pair => new KeyValuePair<int, Direction>(pair.Key + left.Columns.Count, pair.Value)))),
        TupleDescriptor.Create(new[] {left.OrderDescriptor.TupleDescriptor, right.OrderDescriptor.TupleDescriptor}.SelectMany(descriptor => descriptor)));
    }

    public RecordSetHeader(RecordSetHeader header, string alias)
    {
      Columns = new RecordColumnCollection(header.Columns, alias);
      TupleDescriptor = header.TupleDescriptor;
      Keys = header.Keys;

      OrderDescriptor = new RecordSetOrderDescriptor(
        header.OrderDescriptor.Order,
        header.OrderDescriptor.TupleDescriptor);
    }

    public RecordSetHeader(RecordSetHeader header, IEnumerable<string> includedColumns)
    {
      IList<RecordColumn> list = new List<RecordColumn>();
      var keyIndicesByColumn =
        header.Keys.SelectMany(
          (keyInfo, i) => keyInfo.Columns.Select(column => new KeyValuePair<RecordColumn, int>(column, i))).
          ToDictionary(pair => pair.Key, pair => pair.Value);
     
      foreach (string includedColumn in includedColumns) {
        RecordColumn recordColumn = header.Columns[includedColumn];
        list.Add(recordColumn);
      }

      var excludedKeys =
        header.Columns.Except(list).Select(column => header.Keys[keyIndicesByColumn[column]]);

      Columns = new RecordColumnCollection(list);
      TupleDescriptor = header.TupleDescriptor;
      Keys = new CollectionBaseSlim<KeyInfo>(header.Keys.Except(excludedKeys));

      OrderDescriptor = new RecordSetOrderDescriptor(
        header.OrderDescriptor.Order,
        header.OrderDescriptor.TupleDescriptor);
    }
  }
}