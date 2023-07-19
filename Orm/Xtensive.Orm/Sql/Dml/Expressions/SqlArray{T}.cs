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
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlArray<T>>(expression);
      Values = replacingExpression.Values;
    }

    internal override SqlArray Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlArray<T>((T[]) t.Values.Clone()));


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
