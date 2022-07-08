// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlFunctionCall: SqlExpression
  {
    /// <summary>
    /// Gets the expressions.
    /// </summary>
    public IList<SqlExpression> Arguments { get; private set; }

    /// <summary>
    /// Gets the function type.
    /// </summary>
    public SqlFunctionType FunctionType { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlFunctionCall>(expression, "expression");
      var replacingExpression = (SqlFunctionCall) expression;
      FunctionType = replacingExpression.FunctionType;
      Arguments.Clear();
      foreach (SqlExpression argument in replacingExpression.Arguments)
        Arguments.Add(argument);
    }

    internal override SqlFunctionCall Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) => {
        var clone = new SqlFunctionCall(t.FunctionType);
        for (int i = 0, l = t.Arguments.Count; i < l; i++)
          clone.Arguments.Add(t.Arguments[i].Clone(c));
        return clone;
      });

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlFunctionCall(SqlFunctionType functionType, IEnumerable<SqlExpression> arguments)
      : base(SqlNodeType.FunctionCall)
    {
      FunctionType = functionType;
      Arguments = new Collection<SqlExpression>();
      foreach (SqlExpression argument in arguments)
        Arguments.Add(argument);
    }

    internal SqlFunctionCall(SqlFunctionType functionType, params SqlExpression[] arguments)
      : base(SqlNodeType.FunctionCall)
    {
      FunctionType = functionType;
      Arguments = new Collection<SqlExpression>();
      if (arguments != null)
        foreach (SqlExpression argument in arguments)
          Arguments.Add(argument);
    }
  }
}