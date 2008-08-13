// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.13

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private static readonly ThreadSafeDictionary<IndexInfo, RecordSetHeader> headers =
      ThreadSafeDictionary<IndexInfo, RecordSetHeader>.Create(new object());
    private TupleDescriptor orderTupleDescriptor;

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
    /// Gets the indexes of columns <see cref="RecordSet"/> is ordered by.
    /// </summary>
    public DirectionCollection<int> Order { get; private set; }

    /// <summary>
    /// Gets the tuple descriptor describing 
    /// a set of <see cref="Order"/> columns.
    /// </summary>
    public TupleDescriptor OrderTupleDescriptor
    {
      get
      {
        if (orderTupleDescriptor == null && Order.Count > 0) lock(this) if (orderTupleDescriptor==null)
          orderTupleDescriptor = Core.Tuples.TupleDescriptor.Create(Order.Select(p => Columns[p.Key].Type));
        return orderTupleDescriptor;
      }
    }

    /// <summary>
    /// Aliases the header.
    /// </summary>
    /// <param name="alias">The alias to apply.</param>
    /// <returns>Aliased header.</returns>
    public RecordSetHeader Alias(string alias)
    {
      return new RecordSetHeader(this, alias);
    }

    /// <summary>
    /// Joins the header with the specified one.
    /// </summary>
    /// <param name="joined">The header to join.</param>
    /// <returns>The joined header.</returns>
    public RecordSetHeader Join(RecordSetHeader joined)
    {
      return new RecordSetHeader(this, joined);
    }

    /// <summary>
    /// Selects the specified columns from the header.
    /// </summary>
    /// <param name="selected">The indexes of columns to select.</param>
    /// <returns>A new header containing only specified columns.</returns>
    public RecordSetHeader Select(IEnumerable<int> selected)
    {
      return new RecordSetHeader(this, selected);
    }

    /// <summary>
    /// Gets the <see cref="RecordSetHeader"/> object for the specified <paramref name="indexInfo"/>.
    /// </summary>
    /// <param name="indexInfo">The index info to get the header for.</param>
    /// <returns>The <see cref="RecordSetHeader"/> object.</returns>
    public static RecordSetHeader GetHeader(IndexInfo indexInfo)
    {
      return headers.GetValue(indexInfo, _indexInfo => new RecordSetHeader(_indexInfo));
    }
   

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="tupleDescriptor">Descriptor of the result item.</param>
    /// <param name="columns">Result columns.</param>
    /// <param name="groups">Column groups.</param>
    /// <param name="orderedBy">Result sort order.</param>
    public RecordSetHeader(TupleDescriptor tupleDescriptor, IEnumerable<Column> columns, 
      IEnumerable<ColumnGroup> groups, 
      DirectionCollection<int> orderedBy)
    {
      ArgumentValidator.EnsureArgumentNotNull(tupleDescriptor, "tupleDescriptor");
      ArgumentValidator.EnsureArgumentNotNull(columns, "columns");
      TupleDescriptor = tupleDescriptor;
      Columns = new ColumnCollection(columns);

      ColumnGroups = groups==null
        ? ColumnGroupCollection.Empty
        : new ColumnGroupCollection(groups);

      Order = orderedBy ?? new DirectionCollection<int>();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="indexInfo">Index descriptor.</param>
    private RecordSetHeader(IndexInfo indexInfo)
    {
      TupleDescriptor = TupleDescriptor.Create(indexInfo.Columns.Select(columnInfo => columnInfo.ValueType));
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
      Order = sortOrder;
      Columns = new ColumnCollection(indexInfo.Columns.Select((c,i) => new Column(c,i,c.ValueType)));

      ColumnGroups = new ColumnGroupCollection(new[]{new ColumnGroup(
        indexInfo.Group.KeyColumns.Select(c => indexInfo.Columns.IndexOf(c)),
        indexInfo.Group.Columns.Select(c => indexInfo.Columns.IndexOf(c)))});
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <remarks>This constructor is used for all join operations.</remarks>
    private RecordSetHeader(RecordSetHeader left, RecordSetHeader right)
    {
      Columns = new ColumnCollection(
        left.Columns,
        right.Columns.Select(column => new Column(column, left.Columns.Count + column.Index)));
      TupleDescriptor = TupleDescriptor.Create(new[] {left.TupleDescriptor, right.TupleDescriptor}.SelectMany(descriptor => descriptor));
      Order = left.Order;
      orderTupleDescriptor = left.orderTupleDescriptor;
      ColumnGroups = new ColumnGroupCollection(
        left.ColumnGroups
          .Union(right.ColumnGroups
            .Select(g => new ColumnGroup(
              g.Keys.Select(i => left.Columns.Count + i),
              g.Columns.Select(i => left.Columns.Count + i)
              ))));
    }

    private RecordSetHeader(RecordSetHeader header, string alias)
    {
      Columns = new ColumnCollection(header.Columns, alias);
      TupleDescriptor = header.TupleDescriptor;
      ColumnGroups = header.ColumnGroups;
      Order = header.Order;
      orderTupleDescriptor = header.orderTupleDescriptor;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="header">Original header.</param>
    /// <param name="includedColumns">Indexes of columns that will be included in result.</param>
    private RecordSetHeader(RecordSetHeader header, IEnumerable<int> includedColumns)
    {
      var columns = new List<int>(includedColumns);
      TupleDescriptor = Core.Tuples.TupleDescriptor.Create(includedColumns.Select(i => header.TupleDescriptor[i]));
      if (header.Order.Count > 0) {
        Order = new DirectionCollection<int>(
          header.Order
          .Select(o => new KeyValuePair<int, Direction>(columns.IndexOf(o.Key), o.Value))
          .TakeWhile(o => o.Key >= 0));
      }

      Columns = new ColumnCollection(includedColumns.Select((ic, i) => new Column(header.Columns[ic], i)));
      ColumnGroups = new ColumnGroupCollection(
        header.ColumnGroups
        .Where(g => g.Keys.All(ci => includedColumns.Contains(ci)))
        .Select(cgm => new ColumnGroup(cgm.Keys.Select(k => columns.IndexOf(k)), cgm.Columns.Select(c => columns.IndexOf(c)).Where(c => c >=0))));
    }
  }
}