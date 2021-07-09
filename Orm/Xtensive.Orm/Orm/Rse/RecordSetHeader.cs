// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.13

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;

using Xtensive.Orm.Model;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using System.Linq;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Header of <see cref="Provider"/>.
  /// </summary>
  [Serializable]
  public sealed class RecordSetHeader
  {
    private volatile TupleDescriptor orderTupleDescriptor;

    /// <summary>
    /// Gets the length of this instance.
    /// </summary>
    public int Length {
      get { return Columns.Count; }
    }

    /// <summary>
    /// Gets the <see cref="Provider"/> keys.
    /// </summary>
    /// <value>The keys.</value>
    public ColumnGroupCollection ColumnGroups { get; private set; }

    /// <summary>
    /// Gets the <see cref="Provider"/> columns.
    /// </summary>
    public ColumnCollection Columns { get; private set; }

    /// <summary>
    /// Gets the <see cref="Provider"/> tuple descriptor.
    /// </summary>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <summary>
    /// Gets the indexes of columns <see cref="Provider"/> is ordered by.
    /// </summary>
    public DirectionCollection<int> Order { get; private set; }

    /// <summary>
    /// Gets the tuple descriptor describing 
    /// a set of <see cref="Order"/> columns.
    /// </summary>
    public TupleDescriptor OrderTupleDescriptor {
      get {
        if (Order.Count==0)
          return null;
        if (orderTupleDescriptor==null) lock(this) if (orderTupleDescriptor==null) {
          var fieldTypes = Order.Select(p => Columns[p.Key].Type).ToArray(Order.Count);
          orderTupleDescriptor = TupleDescriptor.Create(fieldTypes);
        }

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
    /// Adds the specified column to header.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>The constructed header.</returns>
    public RecordSetHeader Add(Column column)
    {
      return Add(EnumerableUtils.One(column));
    }

    /// <summary>
    /// Adds the specified columns to header.
    /// </summary>
    /// <param name="columns">The columns to add.</param>
    /// <returns>The constructed header.</returns>
    public RecordSetHeader Add(IEnumerable<Column> columns)
    {
      var newColumns = new List<Column>(Columns);
      newColumns.AddRange(columns);

      var newFieldTypes = new Type[newColumns.Count];
      for (var i = 0; i < newColumns.Count; i++)
        newFieldTypes[i] = newColumns[i].Type;
      var newTupleDescriptor = TupleDescriptor.Create(newFieldTypes);

      return new RecordSetHeader(
        newTupleDescriptor, 
        newColumns, 
        ColumnGroups, 
        OrderTupleDescriptor, 
        Order);
    }

    /// <summary>
    /// Joins the header with the specified one.
    /// </summary>
    /// <param name="joined">The header to join.</param>
    /// <returns>The joined header.</returns>
    public RecordSetHeader Join(RecordSetHeader joined)
    {
      var columnCount = Columns.Count;
      var newColumns = new List<Column>(columnCount + joined.Columns.Count);
      newColumns.AddRange(Columns);
      foreach (var c in joined.Columns) {
        newColumns.Add(c.Clone(columnCount + c.Index));
      }

      var newFieldTypes = new Type[newColumns.Count];
      for (var i = 0; i < newColumns.Count; i++)
        newFieldTypes[i] = newColumns[i].Type;
      var newTupleDescriptor = TupleDescriptor.Create(newFieldTypes);

      var columnGroupCount = ColumnGroups.Count;
      var groups = new List<ColumnGroup>(columnGroupCount + joined.ColumnGroups.Count);
      groups.AddRange(ColumnGroups);
      foreach (var g in joined.ColumnGroups) {
        var keys = new List<int>(g.Keys.Count);
        foreach (var i in g.Keys) {
          keys.Add(columnCount + i);
        }
        var columns = new List<int>(g.Columns.Count);
        foreach (var i in g.Columns) {
          columns.Add(columnCount + i);
        }
        groups.Add(new ColumnGroup(g.TypeInfoRef, keys, columns));
      }

      return new RecordSetHeader(
        newTupleDescriptor, 
        newColumns, 
        groups,
        OrderTupleDescriptor, 
        Order);
    }

    /// <summary>
    /// Selects the specified columns from the header.
    /// </summary>
    /// <param name="selectedColumns">The indexes of columns to select.</param>
    /// <returns>A new header containing only specified columns.</returns>
    public RecordSetHeader Select(IEnumerable<int> selectedColumns)
    {
      var columns = new List<int>(selectedColumns);
      var columnsMap = new List<int>(Enumerable.Repeat(-1, Columns.Count));
      for (int newIndex = 0; newIndex < columns.Count; newIndex++) {
        var oldIndex = columns[newIndex];
        columnsMap[oldIndex] = newIndex;
      }

      var fieldTypes = columns.Select(i => TupleDescriptor[i]).ToArray(columns.Count);
      var resultTupleDescriptor = Xtensive.Tuples.TupleDescriptor.Create(fieldTypes);
      var resultOrder = new DirectionCollection<int>(
        Order
          .Select(o => new KeyValuePair<int, Direction>(columnsMap[o.Key], o.Value))
          .TakeWhile(o => o.Key >= 0));      

      var resultColumns = columns.Select((oldIndex, newIndex) => Columns[oldIndex].Clone(newIndex));

      var resultGroups = ColumnGroups
        .Where(g => g.Keys.All(k => columnsMap[k]>=0))
        .Select(g => new ColumnGroup(
            g.TypeInfoRef,
            g.Keys.Select(k => columnsMap[k]), 
            g.Columns
              .Select(c => columnsMap[c])
              .Where(c => c >= 0)));

      return new RecordSetHeader(
        resultTupleDescriptor, 
        resultColumns, 
        resultGroups, 
        null, 
        resultOrder);      
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
      return CreateHeader(indexInfo);
    }

    private static RecordSetHeader CreateHeader(IndexInfo indexInfo)
    {
      var indexInfoColumns = indexInfo.Columns;
      var indexInfoKeyColumns = indexInfo.KeyColumns;

      var resultFieldTypes = indexInfoColumns.Select(columnInfo => columnInfo.ValueType).ToArray(indexInfoColumns.Count);
      var resultTupleDescriptor = TupleDescriptor.Create(resultFieldTypes);

      var keyOrder = new List<KeyValuePair<int, Direction>>(
        indexInfoKeyColumns.Select((p, i) => new KeyValuePair<int, Direction>(i, p.Value)));
      if (!indexInfo.IsPrimary) {
        var pkKeys = indexInfo.ReflectedType.Indexes.PrimaryIndex.KeyColumns;
        keyOrder.AddRange(
          indexInfo.ValueColumns
            .Select((c, i) => new Pair<ColumnInfo, int>(c, i + indexInfoKeyColumns.Count))
            .Where(pair => pair.First.IsPrimaryKey)
            .Select(pair => new KeyValuePair<int, Direction>(pair.Second, pkKeys[pair.First])));
      }
      var order = new DirectionCollection<int>(keyOrder);

      var keyFieldTypes = indexInfoKeyColumns
        .Select(columnInfo => columnInfo.Key.ValueType)
        .ToArray(indexInfoKeyColumns.Count);
      var keyDescriptor = TupleDescriptor.Create(keyFieldTypes);
      
      var resultColumns = indexInfoColumns.Select((c,i) => (Column) new MappedColumn(c,i,c.ValueType));
      var resultGroups = new[]{indexInfo.Group};

      return new RecordSetHeader(
        resultTupleDescriptor, 
        resultColumns, 
        resultGroups, 
        keyDescriptor, 
        order);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return Columns.Select(c => c.ToString()).ToCommaDelimitedString();      
    }
   

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="tupleDescriptor">Descriptor of the result item.</param>
    /// <param name="columns">Result columns.</param>    
    public RecordSetHeader(
      TupleDescriptor tupleDescriptor, 
      IEnumerable<Column> columns)
      : this(tupleDescriptor, columns, null, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="tupleDescriptor">Descriptor of the result item.</param>
    /// <param name="columns">Result columns.</param>    
    /// <param name="columnGroups">Column groups.</param>
    public RecordSetHeader(
      TupleDescriptor tupleDescriptor, 
      IEnumerable<Column> columns, 
      IEnumerable<ColumnGroup> columnGroups)
      : this(tupleDescriptor, columns, columnGroups, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="tupleDescriptor">Descriptor of the result item.</param>
    /// <param name="columns">Result columns.</param>    
    /// <param name="orderKeyDescriptor">Descriptor of ordered columns.</param>
    /// <param name="order">Result sort order.</param>
    public RecordSetHeader(
      TupleDescriptor tupleDescriptor, 
      IEnumerable<Column> columns, 
      TupleDescriptor orderKeyDescriptor,
      DirectionCollection<int> order)      
      : this(tupleDescriptor, columns, null, orderKeyDescriptor, order)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="tupleDescriptor">Descriptor of the result item.</param>
    /// <param name="columns">Result columns.</param>    
    /// <param name="columnGroups">Column groups.</param>
    /// <param name="orderKeyDescriptor">Descriptor of ordered columns.</param>
    /// <param name="order">Result sort order.</param>
    /// <exception cref="ArgumentOutOfRangeException"><c>columns.Count</c> is out of range.</exception>
    public RecordSetHeader(
      TupleDescriptor tupleDescriptor, 
      IEnumerable<Column> columns, 
      IEnumerable<ColumnGroup> columnGroups,
      TupleDescriptor orderKeyDescriptor,
      DirectionCollection<int> order)      
    {
      ArgumentValidator.EnsureArgumentNotNull(tupleDescriptor, "tupleDescriptor");
      ArgumentValidator.EnsureArgumentNotNull(columns, "columns");

      TupleDescriptor = tupleDescriptor;
      // Unsafe perf. optimization: if you pass a list, it should be immutable!
      Columns = columns is List<Column> columnList 
        ? new ColumnCollection(columnList) 
        : new ColumnCollection(columns);
      if (tupleDescriptor.Count!=Columns.Count)
        throw new ArgumentOutOfRangeException("columns.Count");

      ColumnGroups = columnGroups == null
        ? ColumnGroupCollection.Empty
        // Unsafe perf. optimization: if you pass a list, it should be immutable!
        : (columnGroups is List<ColumnGroup> columnGroupList
          ? new ColumnGroupCollection(columnGroupList) 
          : new ColumnGroupCollection(columnGroups));

      orderTupleDescriptor = orderKeyDescriptor ?? TupleDescriptor.Empty;
      Order = order ?? new DirectionCollection<int>();
      Order.Lock(true);
    }
  }
}
