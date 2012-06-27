// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.07.10

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Collections;
using System.Linq;
using Xtensive.Core;

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Describes a single index in terms of storage.
  /// </summary>
  [DebuggerDisplay("{Name}; Attributes = {Attributes}.")]
  [Serializable]
  public sealed class IndexInfo : MappedNode
  {
    private IndexAttributes attributes;
    private ColumnGroup columnGroup;
    private DirectionCollection<ColumnInfo> keyColumns = new DirectionCollection<ColumnInfo>();
    private ColumnInfoCollection valueColumns;
    private ColumnInfoCollection includedColumns;
    private readonly CollectionBaseSlim<IndexInfo> underlyingIndexes = new CollectionBaseSlim<IndexInfo>();
    private readonly TypeInfo declaringType;
    private readonly TypeInfo reflectedType;
    private readonly IndexInfo declaringIndex;
    private double fillFactor;
    private string shortName;
    private ReadOnlyList<ColumnInfo> columns;
    private TupleDescriptor tupleDescriptor;
    private TupleDescriptor keyTupleDescriptor;
    private IEnumerable<TypeInfo> filterByTypes;
    private IList<int> selectColumns;
    private List<Pair<int, List<int>>> valueColumnsMap;
    private LambdaExpression filterExpression;
    private PartialIndexFilterInfo filter;

    /// <summary>
    /// Gets or sets the column index map.
    /// </summary>
    public ColumnIndexMap ColumnIndexMap { get; private set; }

    public string ShortName {
      [DebuggerStepThrough]
      get { return shortName; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        shortName = value;
      }
    }

    public double FillFactor {
      [DebuggerStepThrough]
      get { return fillFactor; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        fillFactor = value;
      }
    }

    public ColumnGroup Group {
      [DebuggerStepThrough]
      get { return columnGroup; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        columnGroup = value;
      }
    }

    /// <summary>
    /// Gets a collection of all the columns that are included into the index.
    /// </summary>
    public ReadOnlyList<ColumnInfo> Columns {
      [DebuggerStepThrough]
      get {
        return columns;
      }
    }

    /// <summary>
    /// Gets a collection of columns that are included into the index as index key.
    /// </summary>
    public DirectionCollection<ColumnInfo> KeyColumns
    {
      [DebuggerStepThrough]
      get { return keyColumns; }
    }

    /// <summary>
    /// Gets a collection of non key columns that are included into the index as index value.
    /// </summary>
    public ColumnInfoCollection ValueColumns
    {
      [DebuggerStepThrough]
      get { return valueColumns; }
    }

    /// <summary>
    /// Gets a Collection of columns that are included into the index.
    /// </summary>
    public ColumnInfoCollection IncludedColumns
    {
      [DebuggerStepThrough]
      get { return includedColumns; }
    }

    /// <summary>
    /// Gets the tuple descriptor containing all the <see cref="Columns"/>.
    /// </summary>
    public TupleDescriptor TupleDescriptor
    {
      [DebuggerStepThrough]
      get { return tupleDescriptor; }
    }

    /// <summary>
    /// Gets the tuple descriptor containing just <see cref="KeyColumns"/>.
    /// </summary>
    public TupleDescriptor KeyTupleDescriptor
    {
      [DebuggerStepThrough]
      get { return keyTupleDescriptor; }
    }

    /// <summary>
    /// Gets the underlying indexes for this instance. 
    /// </summary>
    public CollectionBaseSlim<IndexInfo> UnderlyingIndexes
    {
      [DebuggerStepThrough]
      get { return underlyingIndexes; }
    }

    /// <summary>
    /// Gets the type that declares this member.
    /// </summary>
    public TypeInfo DeclaringType
    {
      [DebuggerStepThrough]
      get { return declaringType; }
    }

    /// <summary>
    /// Gets the type that was used to obtain this instance.
    /// </summary>
    public TypeInfo ReflectedType
    {
      [DebuggerStepThrough]
      get { return reflectedType; }
    }

    /// <summary>
    /// Gets the declaring index for this index.
    /// </summary>
    public IndexInfo DeclaringIndex
    {
      [DebuggerStepThrough]
      get { return declaringIndex; }
    }

    /// <summary>
    /// Gets the types for <see cref="IndexAttributes.Filtered"/> index.
    /// </summary>
    public IEnumerable<TypeInfo> FilterByTypes
    {
      get { return filterByTypes; }
      set
      {
        this.EnsureNotLocked();
        filterByTypes = value;
      }
    }

    /// <summary>
    /// Gets expression that defines range for partial index.
    /// </summary>
    public LambdaExpression FilterExpression {
      [DebuggerStepThrough]
      get { return filterExpression; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        filterExpression = value;
      }
    }

    /// <summary>
    /// Gets filter that defines range for partial index.
    /// This is built upon <see cref="FilterExpression"/>
    /// on late stage of <see cref="DomainModel"/> build.
    /// </summary>
    public PartialIndexFilterInfo Filter {
      [DebuggerStepThrough]
      get { return filter; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        filter = value;
      }
    }

    /// <summary>
    /// Gets the column indexes for <see cref="IndexAttributes.View"/> index.
    /// </summary>
    public IList<int> SelectColumns
    {
      get { return selectColumns; }
      set
      {
        this.EnsureNotLocked();
        selectColumns = value;
      }
    }

    public List<Pair<int, List<int>>> ValueColumnsMap
    {
      get { return valueColumnsMap; }
      set
      {
        this.EnsureNotLocked();
        valueColumnsMap = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is primary index.
    /// </summary>
    public bool IsPrimary
    {
      [DebuggerStepThrough]
      get { return (attributes & IndexAttributes.Primary) > 0; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is typed index.
    /// </summary>
    public bool IsTyped
    {
      [DebuggerStepThrough]
      get { return (attributes & IndexAttributes.Typed) > 0; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is unique index.
    /// </summary>
    public bool IsUnique
    {
      [DebuggerStepThrough]
      get { return (attributes & IndexAttributes.Unique) > 0; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is abstract.
    /// </summary>
    public bool IsAbstract
    {
      [DebuggerStepThrough]
      get { return (attributes & IndexAttributes.Abstract) > 0; }
    }

    /// <summary>
    /// Gets or sets the attributes.
    /// </summary>
    public IndexAttributes Attributes
    {
      [DebuggerStepThrough]
      get { return attributes; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        attributes = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is virtual index.
    /// </summary>
    public bool IsVirtual
    {
      [DebuggerStepThrough]
      get { return (Attributes & IndexAttributes.Virtual) > 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is secondary index.
    /// </summary>
    public bool IsSecondary
    {
      [DebuggerStepThrough]
      get { return (attributes & IndexAttributes.Secondary) > 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is a partial index.
    /// </summary>
    public bool IsPartial
    {
      [DebuggerStepThrough]
      get { return (attributes & IndexAttributes.Partial) > 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is clustered index.
    /// </summary>
    public bool IsClustered
    {
      [DebuggerStepThrough]
      get { return (attributes & IndexAttributes.Clustered) > 0; }
    }

    /// <inheritdoc/>
    public override void UpdateState()
    {
      base.UpdateState();
      CreateColumns();
      valueColumns.UpdateState();
      foreach (IndexInfo baseIndex in underlyingIndexes)
        baseIndex.UpdateState();
      if (filter!=null)
        filter.UpdateState();
      CreateTupleDescriptors();

      if (!IsPrimary)
        return;

      var system = new List<int>();
      var lazy = new List<int>();
      var regular = new List<int>();

      for (int i = 0; i < columns.Count; i++) {
        var item = columns[i];
        if (item.IsPrimaryKey || item.IsSystem)
          system.Add(i);
        else {
          if (item.IsLazyLoad)
            lazy.Add(i);
          else
            regular.Add(i);
        }
      }

      ColumnIndexMap = new ColumnIndexMap(system, regular, lazy);
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (!recursive)
        return;
      keyColumns.Lock();
      valueColumns.Lock();
      if (filter != null)
        filter.Lock();
      foreach (IndexInfo baseIndex in underlyingIndexes)
        baseIndex.Lock();
      underlyingIndexes.Lock();
    }

    public IndexInfo Clone()
    {
      var result = new IndexInfo(reflectedType, attributes, declaringIndex);
      result.shortName = shortName;
      result.Name = Name;
      result.keyColumns = keyColumns;
      result.valueColumns = valueColumns;
      result.includedColumns = includedColumns;
      return result;
    }

    private void CreateTupleDescriptors()
    {
      tupleDescriptor = TupleDescriptor.Create(
        from c in Columns select c.ValueType);
      keyTupleDescriptor = TupleDescriptor.Create(
        from c in KeyColumns select c.Key.ValueType);
    }

    private void CreateColumns()
    {
      var result = new List<ColumnInfo>(keyColumns.Select(pair => pair.Key));
      result.AddRange(valueColumns);
      columns = new ReadOnlyList<ColumnInfo>(result);
    }


    // Constructors

    private IndexInfo()
    {
      includedColumns = new ColumnInfoCollection(this, "IncludedColumns");
      valueColumns = new ColumnInfoCollection(this, "ValueColumns");
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="declaringType">The <see cref="TypeInfo"/> that declares this instance.</param>
    /// <param name="indexAttributes"><see cref="IndexAttributes"/> attributes for this instance.</param>
    public IndexInfo(TypeInfo declaringType, IndexAttributes indexAttributes)
      : this()
    {
      this.declaringType = declaringType;
      attributes = indexAttributes;
      reflectedType = declaringType;
      declaringIndex = this;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="reflectedType">Reflected type.</param>
    /// <param name="ancestorIndex">The ancestors index.</param>
    /// <param name="indexAttributes"><see cref="IndexAttributes"/> attributes for this instance.</param>
    public IndexInfo(TypeInfo reflectedType, IndexAttributes indexAttributes, IndexInfo ancestorIndex)
      : this()
    {
      declaringType = ancestorIndex.DeclaringType;
      this.reflectedType = reflectedType;
      attributes = indexAttributes;

      fillFactor = ancestorIndex.FillFactor;
      filterExpression = ancestorIndex.FilterExpression;
      declaringIndex = ancestorIndex.DeclaringIndex;
      shortName = ancestorIndex.ShortName;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="reflectedType">Reflected type.</param>
    /// <param name="indexAttributes">The index attributes.</param>
    /// <param name="baseIndex">Base index.</param>
    /// <param name="baseIndexes">The base indexes.</param>
    public IndexInfo(TypeInfo reflectedType, IndexAttributes indexAttributes, IndexInfo baseIndex, params IndexInfo[] baseIndexes)
      : this()
    {
      declaringType = baseIndex.DeclaringType;
      this.reflectedType = reflectedType;
      attributes = indexAttributes;

      fillFactor = baseIndex.FillFactor;
      filterExpression = baseIndex.FilterExpression;
      declaringIndex = baseIndex.DeclaringIndex;
      shortName = baseIndex.ShortName;
      
      UnderlyingIndexes.Add(baseIndex);

      foreach (IndexInfo info in baseIndexes)
        UnderlyingIndexes.Add(info);
    }
  }
}
