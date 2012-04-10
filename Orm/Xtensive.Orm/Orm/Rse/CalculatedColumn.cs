// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.09

using System;
using System.Linq.Expressions;
using Xtensive.Core;

using Xtensive.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Calculated column of the <see cref="RecordSetHeader"/>.
  /// </summary>
  [Serializable]
  public sealed class CalculatedColumn : Column
  {
    private const string ToStringFormat = "{0} = {1}";

    /// <summary>
    /// Gets the column expression.
    /// </summary>
    public Expression<Func<Tuple, object>> Expression { get; private set; }

    
    public override string ToString()
    {
      return string.Format(ToStringFormat,
        base.ToString(), Expression.ToString(true));
    }

    
    public override Column Clone(int newIndex)
    {
      return new CalculatedColumn(this, newIndex);
    }

    
    public override Column Clone(string newName)
    {
      return new CalculatedColumn(this, newName);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="descriptor"><see cref="CalculatedColumnDescriptor"/> property value.</param>
    /// <param name="index"><see cref="Column.Index"/> property value.</param>
    public CalculatedColumn(CalculatedColumnDescriptor descriptor, int index)
      : base(descriptor.Name, index, descriptor.Type)
    {
      Expression = descriptor.Expression;
    }

    #region Clone constructors

    private CalculatedColumn(CalculatedColumn column, string newName)
      : base(newName, column.Index, column.Type)
    {
      Expression = column.Expression;
    }
 
    private CalculatedColumn(CalculatedColumn column, int newIndex)
      : base(column.Name, newIndex, column.Type)
    {
      Expression = column.Expression;
    }

    #endregion
  }
}