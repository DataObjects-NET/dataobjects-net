// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.16

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that applies aggregate functions to grouped columns from <see cref="UnaryProvider.Source"/>.
  /// </summary>
  public class UnOrderedGroupProvider : UnaryProvider
  {
    /// <summary>
    /// Gets the aggregate columns.
    /// </summary>
    public AggregateColumn[] AggregateColumns { get; private set; }

    /// <summary>
    /// Gets column indexes to group by.
    /// </summary>
    public int[] GroupColumnIndexes { get; private set; }

    /// <summary>
    /// Gets header resize transform.
    /// </summary>
    public MapTransform ResizeTransform { get; private set; }
    
    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      var types = new List<Type>();
      foreach (var index in GroupColumnIndexes)
        types.Add(Source.Header.Columns[index].Type);
      foreach (var col in AggregateColumns)
        types.Add(col.Type);

      var rs = Source.Header.Select(GroupColumnIndexes);
      return new RecordSetHeader(TupleDescriptor.Create(types), rs.Add(AggregateColumns).Columns, null, null, null);
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      var types = new List<Type>();
      int i = 0;
      var columnIndexes = new int[GroupColumnIndexes.Length];
      foreach (var index in GroupColumnIndexes) {
        types.Add(Source.Header.Columns[index].Type);
        columnIndexes[i++] = index;
      }
      ResizeTransform = new MapTransform(false, TupleDescriptor.Create(types), columnIndexes);
    }

    
    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="columnDescriptors">The descriptors of <see cref="AggregateColumns"/>.</param>
    /// <param name="groupIndexes">The column indexes to group by.</param>
    public UnOrderedGroupProvider(CompilableProvider source, AggregateColumnDescriptor[] columnDescriptors, params int[] groupIndexes)
      : base(source)
    {
      var columns = new AggregateColumn[columnDescriptors.Length];
      for (int i = 0; i < columnDescriptors.Length; i++) {
        AggregateColumnDescriptor descriptor = columnDescriptors[i];
        Type type = descriptor.AggregateType == AggregateType.Count ? typeof(long) : Source.Header.Columns[descriptor.SourceIndex].Type;
        var col = new AggregateColumn(descriptor, groupIndexes.Length + i, type);
        columns.SetValue(col, i);
      }
      AggregateColumns = columns;
      GroupColumnIndexes = groupIndexes;
    }
  }
}