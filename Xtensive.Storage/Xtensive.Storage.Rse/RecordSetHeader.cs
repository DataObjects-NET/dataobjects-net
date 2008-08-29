// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.13

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
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
    public ColumnGroupCollection ColumnGroups { get; private set; }

    /// <summary>
    /// Gets the <see cref="RecordSet"/> columns.
    /// </summary>
    public ColumnCollection Columns { get; private set; }

    /// <summary>
    /// Gets the <see cref="RecordSet"/> tuple descriptor.
    /// </summary>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <summary>
    /// Gets the <see cref="RecordSet"/> order descriptor.
    /// </summary>
    public RecordSetOrderDescriptor OrderDescriptor { get; private set; }

   
    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="indexInfo">Index descriptor.</param>
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
      Columns = new ColumnCollection(indexInfo.Columns.Select((c,i) => new Column(c,i,c.ValueType)));

      ColumnGroups = new ColumnGroupCollection(new[]{new ColumnGroup(
        indexInfo.Group.KeyColumns.Select(c => indexInfo.Columns.IndexOf(c)),
        indexInfo.Group.Columns.Select(c => indexInfo.Columns.IndexOf(c)))});
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="tupleDescriptor">Descriptor of the result item.</param>
    /// <param name="columns">Result columns.</param>
    /// <param name="keyDescriptor">Descriptor of ordered columns.</param>
    /// <param name="groups">Column groups.</param>
    /// <param name="orderedBy">Result sort order.</param>
    public RecordSetHeader(TupleDescriptor tupleDescriptor, IEnumerable<Column> columns, TupleDescriptor keyDescriptor, IEnumerable<ColumnGroup> groups, DirectionCollection<int> orderedBy)
    {
      ArgumentValidator.EnsureArgumentNotNull(tupleDescriptor, "tupleDescriptor");
      ArgumentValidator.EnsureArgumentNotNull(columns, "columns");
      TupleDescriptor = tupleDescriptor;
      Columns = new ColumnCollection(columns);

      ColumnGroups = groups==null
        ? ColumnGroupCollection.Empty
        : new ColumnGroupCollection(groups);

      OrderDescriptor = new RecordSetOrderDescriptor(
        orderedBy ?? new DirectionCollection<int>(),
        keyDescriptor ?? TupleDescriptor.Empty);
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <remarks>This constructor is used for all join operations.</remarks>
    public RecordSetHeader(RecordSetHeader left, RecordSetHeader right)
    {
      Columns = new ColumnCollection(
        left.Columns,
        right.Columns.Select(column => new Column(column, left.Columns.Count + column.Index)));
      TupleDescriptor = TupleDescriptor.Create(new[] {left.TupleDescriptor, right.TupleDescriptor}.SelectMany(descriptor => descriptor));
      OrderDescriptor = left.OrderDescriptor;
      ColumnGroups = new ColumnGroupCollection(
        left.ColumnGroups
          .Union(right.ColumnGroups
            .Select(g => new ColumnGroup(
              g.Keys.Select(i => left.Columns.Count + i),
              g.Columns.Select(i => left.Columns.Count + i)
              ))));
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="header">Original header.</param>
    /// <param name="alias">Alias.</param>
    public RecordSetHeader(RecordSetHeader header, string alias)
    {
      Columns = new ColumnCollection(header.Columns, alias);
      TupleDescriptor = header.TupleDescriptor;
      ColumnGroups = header.ColumnGroups;
      OrderDescriptor = header.OrderDescriptor;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="header">Original header.</param>
    /// <param name="includedColumns">Indexes of columns that will be included in result.</param>
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

      Columns = new ColumnCollection(includedColumns.Select((ic, i) => new Column(header.Columns[ic], i)));
      ColumnGroups = new ColumnGroupCollection(
        header.ColumnGroups
        .Where(g => g.Keys.All(ci => includedColumns.Contains(ci)))
        .Select(cgm => new ColumnGroup(cgm.Keys.Select(k => columns.IndexOf(k)), cgm.Columns.Select(c => columns.IndexOf(c)).Where(c => c >=0))));
    }
  }
}