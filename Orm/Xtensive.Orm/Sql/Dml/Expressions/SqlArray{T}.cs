// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents generic array expression.
  /// </summary>
  [Serializable]
  public class SqlArray<T> : SqlArray
  {
    /// <summary>
    /// Gets the values.
    /// </summary>
    /// <values>The values.</values>
    public T[] Values { get; private set; }

    public override Type ItemType { get { return typeof (T); } }

    public override int Length { get { return Values.Length; } }

    public override object[] GetValues()
    {
      return Values.Cast<object>().ToArray();
    }
    
    public static implicit operator SqlArray<T>(T[] value)
    {
      return new SqlArray<T>(value);
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlArray<T>>(expression, "expression");
      var replacingExpression = (SqlArray<T>) expression;
      Values = replacingExpression.Values;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];
      var clone = new SqlArray<T>((T[]) Values.Clone());
      context.NodeMapping[this] = clone;
      return clone;
    }


    // Constructors

    internal SqlArray(T[] values)
    {
      Values = values;
    }

    internal SqlArray(List<object> values)
    {
      Values = values.Cast<T>().ToArray();
    }
  }
}
