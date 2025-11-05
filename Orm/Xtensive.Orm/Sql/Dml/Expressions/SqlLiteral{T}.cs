// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents literal value expression.
  /// </summary>
  [Serializable]
  public class SqlLiteral<T> : SqlLiteral
  {
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>The value.</value>
    public T Value { get; private set; }

    public override Type LiteralType { get { return typeof (T); } }

    public override object GetValue()
    {
      return Value;
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlLiteral<T>>(expression);
      Value = replacingExpression.Value;
    }

    internal override SqlLiteral<T> Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlLiteral<T>(t.Value));

    // Constructor

    internal SqlLiteral(T value)
    {
      Value = value;
    }
  }
}
