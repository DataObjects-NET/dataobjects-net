// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.13

using System;
using System.Collections.Generic;
using System.Text;
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
      ColumnCollection resultColumns = Columns.Alias(alias);

      return new RecordSetHeader(
        TupleDescriptor, resultColumns, ColumnGroups, OrderTupleDescriptor, Order);
    }

    /// <summary>
    /// Joins the header with the specified one.
    /// </summary>
    /// <param name="joined">The header to join.</param>
    /// <returns>The joined header.</returns>
    public RecordSetHeader Join(RecordSetHeader joined)
    {
      ColumnCollection resultColumns = 
        Columns.Join(joined.Columns.Select(column => new Column(column, Columns.Count + column.Index)));

      TupleDescriptor resultTupleDescriptor = 
        TupleDescriptor.Create(new[] {TupleDescriptor, joined.TupleDescriptor}.SelectMany(descriptor => descriptor));      

      ColumnGroupCollection resultGroups = new ColumnGroupCollection(
        ColumnGroups
          .Union(joined.ColumnGroups
            .Select(g => new ColumnGroup(
              g.Keys.Select(i => Columns.Count + i),
              g.Columns.Select(i => Columns.Count + i)
              ))));

      return new RecordSetHeader(
        resultTupleDescriptor, resultColumns, resultGroups, OrderTupleDescriptor, Order);
    }

    /// <summary>
    /// Selects the specified columns from the header.
    /// </summary>
    /// <param name="selectedColumns">The indexes of columns to select.</param>
    /// <returns>A new header containing only specified columns.</returns>
    public RecordSetHeader Select(IEnumerable<int> selectedColumns)
    {
      var columns = new List<int>(selectedColumns);

      TupleDescriptor resultTupleDescriptor = Core.Tuples.TupleDescriptor.Create(selectedColumns.Select(i => TupleDescriptor[i]));

      DirectionCollection<int> resultOrder = 
        new DirectionCollection<int>(Order
          .Select(o => new KeyValuePair<int, Direction>(columns.IndexOf(o.Key), o.Value))
          .TakeWhile(o => o.Key >= 0));      

      ColumnCollection resultColumns =
        new ColumnCollection(selectedColumns.Select((ic, i) => new Column(Columns[ic], i)));

      ColumnGroupCollection resultGroups = new ColumnGroupCollection(
        ColumnGroups
          .Where(g => g.Keys.All(ci => selectedColumns.Contains(ci)))
          .Select(cgm => 
            new ColumnGroup(
              cgm.Keys.Select(k => columns.IndexOf(k)), 
              cgm.Columns.Select(c => columns.IndexOf(c)).Where(c => c >= 0))));

      return new RecordSetHeader(
        resultTupleDescriptor, resultColumns, resultGroups, null, resultOrder);      
    }

    /// <summary>
    /// Sorts the header in the specified order.
    /// </summary>
    /// <param name="sortOrder">Order to sort this header in.</param>
    /// <returns>A new sorted header.</returns>
    public RecordSetHeader Sort(DirectionCollection<int> sortOrder)
    {
      return new RecordSetHeader(
        TupleDescriptor,
        Columns,
        ColumnGroups,
        TupleDescriptor,
        sortOrder);
    }


    /// <summary>
    /// Gets the <see cref="RecordSetHeader"/> object for the specified <paramref name="indexInfo"/>.
    /// </summary>
    /// <param name="indexInfo">The index info to get the header for.</param>
    /// <returns>The <see cref="RecordSetHeader"/> object.</returns>
    public static RecordSetHeader GetHeader(IndexInfo indexInfo)
    {      
      return headers.GetValue(indexInfo, CreateHeader);
    }

    private static RecordSetHeader CreateHeader(IndexInfo indexInfo)
    {
      TupleDescriptor resultTupleDescriptor = 
        TupleDescriptor.Create(indexInfo.Columns.Select(columnInfo => columnInfo.ValueType));      

      DirectionCollection<int> sortOrder;
      if (indexInfo.IsPrimary)
        sortOrder = 
          new DirectionCollection<int>(
            indexInfo.KeyColumns.Select((pair, i) => new KeyValuePair<int, Direction>(i, pair.Value)));
      else {
        var keyColumns = indexInfo.ReflectedType.Indexes.PrimaryIndex.KeyColumns;
        sortOrder = new DirectionCollection<int>(
          indexInfo.KeyColumns
          .Select((p, i) => new KeyValuePair<int, Direction>(i, p.Value))
          .Union(keyColumns.Select((p, i) => new KeyValuePair<int, Direction>(i+keyColumns.Count, p.Value))));
      }

      TupleDescriptor keyDescriptor = TupleDescriptor.Create(indexInfo.KeyColumns.Select(columnInfo => columnInfo.Key.ValueType));


      ColumnCollection resultColumns =
        new ColumnCollection(indexInfo.Columns.Select((c,i) => new Column(c,i,c.ValueType)));

      ColumnGroupCollection resultGroups =
        new ColumnGroupCollection(new[]{new ColumnGroup(
          indexInfo.Group.KeyColumns.Select(c => indexInfo.Columns.IndexOf(c)),
          indexInfo.Group.Columns.Select(c => indexInfo.Columns.IndexOf(c)))});

      return 
        new RecordSetHeader(resultTupleDescriptor, resultColumns, resultGroups, keyDescriptor, sortOrder);
    }


    public override string ToString()
    {
      return Columns.Select(c => c.ToString()).ToCommaDelimitedString();      
    }
   

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="tupleDescriptor">Descriptor of the result item.</param>
    /// <param name="columns">Result columns.</param>    
    /// <param name="groups">Column groups.</param>
    /// <param name="orderKeyDescriptor">Descriptor of ordered columns.</param>
    /// <param name="order">Result sort order.</param>
    public RecordSetHeader(
      TupleDescriptor tupleDescriptor, 
      IEnumerable<Column> columns, 
      IEnumerable<ColumnGroup> groups,
      TupleDescriptor orderKeyDescriptor,
      DirectionCollection<int> order)      
    {
      ArgumentValidator.EnsureArgumentNotNull(tupleDescriptor, "tupleDescriptor");
      ArgumentValidator.EnsureArgumentNotNull(columns, "columns");      

      TupleDescriptor = tupleDescriptor;
      Columns = new ColumnCollection(columns);

      ColumnGroups = groups==null
        ? ColumnGroupCollection.Empty
        : new ColumnGroupCollection(groups);

      this.orderTupleDescriptor = orderKeyDescriptor;
      this.Order = order ?? new DirectionCollection<int>();
    }
  }
}
