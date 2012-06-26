// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.13

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Model;
using Xtensive.Threading;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using System.Linq;
using ColumnInfo = Xtensive.Storage.Model.ColumnInfo;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Header of <see cref="RecordQuery"/>.
  /// </summary>
  [Serializable]
  public sealed class RecordSetHeader
  {
    private static readonly ThreadSafeDictionary<IndexInfo, RecordSetHeader> headers =
      ThreadSafeDictionary<IndexInfo, RecordSetHeader>.Create(new object());

    private volatile TupleDescriptor orderTupleDescriptor;

    /// <summary>
    /// Gets the length of this instance.
    /// </summary>
    public int Length {
      get { return Columns.Count; }
    }

    /// <summary>
    /// Gets the <see cref="RecordQuery"/> keys.
    /// </summary>
    /// <value>The keys.</value>
    public ColumnGroupCollection ColumnGroups { get; private set; }

    /// <summary>
    /// Gets the <see cref="RecordQuery"/> columns.
    /// </summary>
    public ColumnCollection Columns { get; private set; }

    /// <summary>
    /// Gets the <see cref="RecordQuery"/> tuple descriptor.
    /// </summary>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <summary>
    /// Gets the indexes of columns <see cref="RecordQuery"/> is ordered by.
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
        if (orderTupleDescriptor==null) lock(this) if (orderTupleDescriptor==null)
          orderTupleDescriptor = TupleDescriptor.Create(Order.Select(p => Columns[p.Key].Type));
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
    /// <param name="columns">The columns.</param>
    /// <returns>The constructed header.</returns>
    public RecordSetHeader Add(IEnumerable<Column> columns)
    {
      var resultColumns = new List<Column>(Columns);
      resultColumns.AddRange(columns);
      var typeList = new List<Type>(TupleDescriptor);
      foreach (var value in columns)
        typeList.Add(value.Type);
      var resultTupleDescriptor = TupleDescriptor.Create(typeList);
      return new RecordSetHeader(
        resultTupleDescriptor, 
        resultColumns, 
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
      var columns = new List<Column>(Columns);
      var originalColumnsCount = Columns.Count;
      columns.AddRange(
        from c in joined.Columns 
        select c.Clone(originalColumnsCount + c.Index));

      var types = new List<Type>(TupleDescriptor);
      types.AddRange(joined.TupleDescriptor);

      var groups = new List<ColumnGroup>(ColumnGroups);
      groups.AddRange(
        joined.ColumnGroups
          .Select(g => new ColumnGroup(
            g.TypeInfoRef,
            g.Keys.Select(i => originalColumnsCount + i),
            g.Columns.Select(i => originalColumnsCount + i))));
      
      return new RecordSetHeader(
        TupleDescriptor.Create(types), 
        columns, 
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

      var resultTupleDescriptor = Xtensive.Tuples.TupleDescriptor.Create(columns.Select(i => TupleDescriptor[i]));
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
      TupleDescriptor resultTupleDescriptor = TupleDescriptor.Create(
        indexInfo.Columns.Select(columnInfo => columnInfo.ValueType));

      var keyOrder = new List<KeyValuePair<int, Direction>>(
        indexInfo.KeyColumns.Select((p, i) => new KeyValuePair<int, Direction>(i, p.Value)));
      
      if (!indexInfo.IsPrimary) {
        var pkKeys = indexInfo.ReflectedType.Indexes.PrimaryIndex.KeyColumns;
        keyOrder.AddRange(
          indexInfo.ValueColumns
            .Select((c, i) => new Pair<Orm.Model.ColumnInfo, int>(c, i + indexInfo.KeyColumns.Count))
            .Where(pair => pair.First.IsPrimaryKey)
            .Select(pair => new KeyValuePair<int, Direction>(pair.Second, pkKeys[pair.First])));
      }

      var order = new DirectionCollection<int>(keyOrder);
      var keyDescriptor = TupleDescriptor.Create(indexInfo.KeyColumns.Select(columnInfo => columnInfo.Key.ValueType));
      var resultColumns = indexInfo.Columns.Select((c,i) => (Column) new MappedColumn(c,i,c.ValueType));
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="tupleDescriptor">Descriptor of the result item.</param>
    /// <param name="columns">Result columns.</param>    
    /// <param name="groups">Column groups.</param>
    public RecordSetHeader(
      TupleDescriptor tupleDescriptor, 
      IEnumerable<Column> columns, 
      IEnumerable<ColumnGroup> groups)
      : this(tupleDescriptor, columns, groups, null, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="tupleDescriptor">Descriptor of the result item.</param>
    /// <param name="columns">Result columns.</param>    
    /// <param name="groups">Column groups.</param>
    /// <param name="orderKeyDescriptor">Descriptor of ordered columns.</param>
    /// <param name="order">Result sort order.</param>
    /// <exception cref="ArgumentOutOfRangeException"><c>columns.Count</c> is out of range.</exception>
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
      if (tupleDescriptor.Count!=Columns.Count)
        throw new ArgumentOutOfRangeException("columns.Count");

      ColumnGroups = groups==null
        ? ColumnGroupCollection.Empty
        : new ColumnGroupCollection(groups);

      orderTupleDescriptor = orderKeyDescriptor ?? TupleDescriptor.Empty;
      Order = order ?? new DirectionCollection<int>();
      Order.Lock(true);
    }
  }
}
