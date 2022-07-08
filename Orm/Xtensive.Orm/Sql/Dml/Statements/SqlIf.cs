// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents SQL IF...ELSE statement.
  /// </summary>
  [Serializable]
  public class SqlIf: SqlStatement, ISqlCompileUnit
  {
    private SqlStatement trueStatement;
    private SqlStatement falseStatement;
    private SqlExpression condition;

    /// <summary>
    /// Gets or sets the condition.
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

    /// <summary>
    /// Gets or sets SQL statement which is carried out if the condition is true.
    /// </summary>
    public SqlStatement True {
      get {
        return trueStatement;
      }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        trueStatement = value;
      }
    }

    /// <summary>
    /// Gets or sets SQL statement which is carried out if the condition is false.
    /// </summary>
    public SqlStatement False {
      get {
        return falseStatement;
      }
      set {
        falseStatement = value;
      }
    }

    internal override SqlIf Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlIf(t.condition.Clone(c),
            (SqlStatement) t.trueStatement.Clone(c),
            t.falseStatement == null ? null : (SqlStatement) t.falseStatement.Clone(c)));

    internal SqlIf(SqlExpression condition, SqlStatement trueStatement, SqlStatement falseStatement)
      : base(SqlNodeType.Conditional)
    {
      this.condition = condition;
      this.trueStatement = trueStatement;
      this.falseStatement = falseStatement;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
