// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

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

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.TryGetValue(this, out var value)) {
        return value;
      }
      
      SqlWhile clone = new SqlWhile((SqlExpression)condition.Clone(context));
      if (statement!=null)
        clone.Statement = (SqlStatement) statement.Clone(context);
      context.NodeMapping[this] = clone;

      return clone;
    }
    
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
