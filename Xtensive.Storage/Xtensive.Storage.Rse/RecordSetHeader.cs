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
  // TODO: Make ColumnGroupMappings and Columns immutable.
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
    public RecordColumnGroupMappingCollection ColumnGroupMappings { get; private set; }

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

   
    // Constructors

    public RecordSetHeader(IndexInfo indexInfo)
    {
      TupleDescriptor = TupleDescriptor.Create(indexInfo.Columns.Select(columnInfo => columnInfo.ValueType));
      var keyDescriptor = TupleDescriptor.Create(indexInfo.KeyColumns.Select(columnInfo => columnInfo.Key.ValueType));
      DirectionCollection<int> sortOrder;
      if (indexInfo.IsPrimary)
        sortOrder = new DirectionCollection<int>(indexInfo.KeyColumns.Select((pair, i) => new KeyValuePair<int, Direction>(i, pair.Value)));
      else {
        var keyColumns = indexInfo.ReflectedType.Indexes.PrimaryIndex.KeyColumns;
        sortOrder = new DirectionCollection<int>(
          indexInfo.KeyColumns
          .Select((p, i) => new KeyValuePair<int, Direction>(i, p.Value))
          .Union(keyColumns.Select((p, i) => new KeyValuePair<int, Direction>(i+keyColumns.Count, p.Value))));
      }
      OrderDescriptor = new RecordSetOrderDescriptor(sortOrder, keyDescriptor);
      Columns = new RecordColumnCollection(indexInfo.Columns.Select((c,i) => new RecordColumn(c,i,c.ValueType)));

      ColumnGroupMappings = new RecordColumnGroupMappingCollection(
        indexInfo.ColumnGroups.Select(cg => new RecordColumnGroupMapping(
          cg.KeyColumns.Select(c => indexInfo.Columns.IndexOf(c)), 
          cg.Columns.Select(c => indexInfo.Columns.IndexOf(c)))));      
    }

    public RecordSetHeader(TupleDescriptor tupleDescriptor, IEnumerable<RecordColumn> recordColumns, TupleDescriptor keyDescriptor, IEnumerable<RecordColumnGroupMapping> keys, DirectionCollection<int> orderedBy)
    {
      ArgumentValidator.EnsureArgumentNotNull(tupleDescriptor, "tupleDescriptor");
      ArgumentValidator.EnsureArgumentNotNull(recordColumns, "recordColumns");
      TupleDescriptor = tupleDescriptor;
      Columns = new RecordColumnCollection(recordColumns);

      ColumnGroupMappings = keys==null
        ? RecordColumnGroupMappingCollection.Empty
        : new RecordColumnGroupMappingCollection(keys);

      OrderDescriptor = new RecordSetOrderDescriptor(
        orderedBy ?? new DirectionCollection<int>(),
        keyDescriptor ?? TupleDescriptor.Empty);
    }

    public RecordSetHeader(RecordSetHeader left, RecordSetHeader right)
    {
      Columns = new RecordColumnCollection(
        left.Columns,
        right.Columns.Select(column => new RecordColumn(column, left.Columns.Count + column.Index)));
      TupleDescriptor = TupleDescriptor.Create(new[] {left.TupleDescriptor, right.TupleDescriptor}.SelectMany(descriptor => descriptor));
      OrderDescriptor = left.OrderDescriptor;
      ColumnGroupMappings = new RecordColumnGroupMappingCollection(
        left.ColumnGroupMappings
          .Union(right.ColumnGroupMappings
            .Select(g => new RecordColumnGroupMapping(
              g.Keys.Select(i => left.Columns.Count + i),
              g.Columns.Select(i => left.Columns.Count + i)
              ))));
    }

    public RecordSetHeader(RecordSetHeader header, string alias)
    {
      Columns = new RecordColumnCollection(header.Columns, alias);
      TupleDescriptor = header.TupleDescriptor;
      ColumnGroupMappings = header.ColumnGroupMappings;
      OrderDescriptor = header.OrderDescriptor;
    }

    public RecordSetHeader(RecordSetHeader header, IEnumerable<int> includedColumns)
    {
      var columns = new List<int>(includedColumns);
      TupleDescriptor = Core.Tuples.TupleDescriptor.Create(includedColumns.Select(i => header.TupleDescriptor[i]));
      if (header.OrderDescriptor.Order.Count > 0) {
        var order = new DirectionCollection<int>(
          header.OrderDescriptor.Order
          .Select(o => new KeyValuePair<int, Direction>(columns.IndexOf(o.Key), o.Value))
          .TakeWhile(o => o.Key >= 0));
        OrderDescriptor = order.Count == 0 ?
          new RecordSetOrderDescriptor(new DirectionCollection<int>(), TupleDescriptor.Empty) :
          new RecordSetOrderDescriptor(order, Core.Tuples.TupleDescriptor.Create(header.OrderDescriptor.Descriptor.Take(order.Count)));
      }

      Columns = new RecordColumnCollection(includedColumns.Select(i => header.Columns[i]));
      ColumnGroupMappings = new RecordColumnGroupMappingCollection(
        header.ColumnGroupMappings.Where(g => g.Keys.All(ci => includedColumns.Contains(ci))));
    }
  }
}