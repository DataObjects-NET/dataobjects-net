// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.11

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that applies aggregate functions to columns from <see cref="UnaryProvider.Source"/>.
  /// </summary>
  public class AggregateProvider : UnaryProvider
  {
    /// <summary>
    /// Gets the aggregate columns.
    /// </summary>
    public AggregateColumn[] AggregateColumns { get; private set; }
    
    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      var types = new List<Type>();
      foreach (var col in AggregateColumns)
        types.Add(col.Type);
      return new RecordSetHeader(TupleDescriptor.Create(types), AggregateColumns, null, null, null);
    }
    

    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="columnDescriptors">The descriptors of <see cref="AggregateColumns"/>.</param>
    public AggregateProvider(CompilableProvider source, params AggregateColumnDescriptor[] columnDescriptors)
      : base(source)
    {
      var columns = new AggregateColumn[columnDescriptors.Length];
      for (int i = 0; i < columnDescriptors.Length; i++) {
        AggregateColumnDescriptor descriptor = columnDescriptors[i];
        Type type = descriptor.AggregateType==AggregateType.Count ? typeof (long) : Source.Header.Columns[descriptor.SourceIndex].Type;
        var col = new AggregateColumn(descriptor, i, type);
        columns.SetValue(col, i);
      }
      AggregateColumns = columns;
    }
  }
}