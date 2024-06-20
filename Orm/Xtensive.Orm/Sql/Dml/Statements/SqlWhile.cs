// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents SQL while loop.
  /// </summary>
  [Serializable]
  public class SqlWhile : SqlStatement
  {
    private SqlStatement statement;
    private SqlExpression condition;

    /// <summary>
    /// Gets or sets the statement to execute.
    /// </summary>
    public SqlStatement Statement {
      get {
        return statement;
      }
      set {
        statement = value;
      }
    }

    /// <summary>
    /// Gets or sets the condition for the repeated execution
    /// of an SQL statement or statement block.
    /// </summary>
    public SqlExpression Condition {
      get {
        return condition;
      }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        SqlValidator.EnsureIsBooleanExpression(value);
        condition = value;
      }
    }

    /// <inheritdoc />
    internal override SqlWhile Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlWhile(t.condition.Clone(c)) {
          Statement = t.statement?.Clone(c)
        });

    internal SqlWhile(SqlExpression condition) : base(SqlNodeType.While)
    {
      this.condition = condition;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
