// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.11

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Rse;


namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Calculated column of the record.
  /// </summary>
  public class AggregateColumn : Column
  {
    /// <summary>
    /// Gets the aggregate function.
    /// </summary>
    public AggregateType AggregateType { get; private set; }

    /// <summary>
    /// Gets the source column index.
    /// </summary>
    public int SourceIndex { get; private set; }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="descriptor"><see cref="AggregateColumnDescriptor"/> property value.</param>
    /// <param name="index"><see cref="SourceIndex"/> property value.</param>
    /// <param name="type"><see cref="Column.Type"/> property value.</param>
    public AggregateColumn(AggregateColumnDescriptor descriptor, int index, Type type)
      : base(descriptor.Name, index, type)
    {
      AggregateType = descriptor.AggregateType;
      SourceIndex = descriptor.SourceIndex;
    }

        /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="column">The original <see cref="AggregateColumn"/> value.</param>
    /// <param name="alias">The alias to add.</param>
    public AggregateColumn(AggregateColumn column, string alias)
      : base(alias.IsNullOrEmpty() ? column.Name : string.Concat(alias, ".", column.Name), column.Index, column.Type)
    {
      AggregateType = column.AggregateType;
      SourceIndex = column.SourceIndex;
    }
  }
}