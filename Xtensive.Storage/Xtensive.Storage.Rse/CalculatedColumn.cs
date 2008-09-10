// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.09

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Calculated column of the record.
  /// </summary>
  public class CalculatedColumn : Column
  {
    /// <summary>
    /// Gets the column expression.
    /// </summary>
    public Func<Tuple, object> Expression { get; private set; }


    // Constructor

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="descriptor"><see cref="CalculatedColumnDescriptor"/> property value.</param>
    /// <param name="index"><see cref="Column.Index"/> property value.</param>
    public CalculatedColumn(CalculatedColumnDescriptor descriptor, int index)
      : base(descriptor.Name, index, descriptor.Type)
    {
      Expression = descriptor.Expression;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="column">The original <see cref="Column"/> property value.</param>
    /// <param name="alias">The alias to add.</param>
    public CalculatedColumn(CalculatedColumn column, string alias)
      : base(alias.IsNullOrEmpty() ? column.Name : string.Concat(alias, ".", column.Name), column.Index, column.Type)
    {
      Expression = column.Expression;
    }
  }
}