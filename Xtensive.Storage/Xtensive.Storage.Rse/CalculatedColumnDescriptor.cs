// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.09

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using System.Linq.Expressions;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Descriptor of the calculated column.
  /// </summary>
  public class CalculatedColumnDescriptor
  {
    /// <summary>
    /// Gets the column name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the column type.
    /// </summary>
    public Type Type { get; private set; }

    /// <summary>
    /// Gets the column expression.
    /// </summary>
    public Expression<Func<Tuple, object>> Expression { get; private set; }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The <see cref="Name"/> property value.</param>
    /// <param name="type">The <see cref="Type"/> property value.</param>
    /// <param name="expression">The <see cref="Expression"/> property value.</param>
    public CalculatedColumnDescriptor(string name, Type type, Expression<Func<Tuple, object>> expression)
    {
      Name = name;
      Type = type;
      Expression = expression;
    }
  }
}