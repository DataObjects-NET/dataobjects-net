// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.06.11

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents a container that can hold any value.
  /// An attempt to its translation leads to an error.
  /// This class can be used to store temporary values inside <see cref="SqlExpression"/>.
  /// </summary>
  public class SqlContainer : SqlExpression
  {
    /// <summary>
    /// Gets the value contained in this instance.
    /// </summary>
    public object Value { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlContainer>(expression);
      Value = replacingExpression.Value;
    }

    internal override SqlContainer Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlContainer(t.Value));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      throw new NotSupportedException();
    }

    internal SqlContainer(object value)
      : base(SqlNodeType.Container)
    {
      Value = value;
    }
  }
}