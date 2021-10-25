// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.06

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlCustomFunctionCall : SqlExpression
  {
    /// <summary>
    /// Gets the custom function type.
    /// </summary>
    public SqlCustomFunctionType FunctionType { get; private set; }

    /// <summary>
    /// Gets the expressions.
    /// </summary>
    public IList<SqlExpression> Arguments { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlCustomFunctionCall>(expression, "expression");
      var replacingExpression = (SqlCustomFunctionCall) expression;
      FunctionType = replacingExpression.FunctionType;
      Arguments.Clear();
      foreach (SqlExpression argument in replacingExpression.Arguments)
        Arguments.Add(argument);
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.TryGetValue(this, out var value)) {
        return value;
      }

      var clone = new SqlCustomFunctionCall(FunctionType);
      for (int i = 0, l = Arguments.Count; i < l; i++)
        clone.Arguments.Add((SqlExpression) Arguments[i].Clone(context));
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    public SqlCustomFunctionCall(SqlCustomFunctionType sqlCustomFunctionType, IEnumerable<SqlExpression> arguments)
      : base(SqlNodeType.CustomFunctionCall)
    {
      FunctionType = sqlCustomFunctionType;
      Arguments = new Collection<SqlExpression>();
      foreach (SqlExpression argument in arguments)
        Arguments.Add(argument);
    }

    public SqlCustomFunctionCall(SqlCustomFunctionType sqlCustomFunctionType, params SqlExpression[] arguments)
      : base(SqlNodeType.CustomFunctionCall)
    {
      FunctionType = sqlCustomFunctionType;
      Arguments = new Collection<SqlExpression>();
      if (arguments!=null)
        foreach (SqlExpression argument in arguments)
          Arguments.Add(argument);
    }
  }
}
