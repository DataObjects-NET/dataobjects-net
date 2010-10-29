// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.09

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Linq;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Descriptor of the calculated column.
  /// </summary>
  [Serializable]
  public class CalculatedColumnDescriptor
  {
    private const string ToStringFormat = "{0} {1} = {2}";

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

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat,
        Type.GetShortName(), Name, Expression.ToString(true));
    }


    // Constructors

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