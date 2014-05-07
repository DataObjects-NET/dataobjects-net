// Copyright (C) 2003-2010 Xtensive LLC.
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
  public class CustomSqlFunctionCall : SqlExpression
  {
    /// <summary>
    /// Gets the expressions.
    /// </summary>
    public IList<SqlExpression> Arguments { get; private set; }

    /// <summary>
    /// Gets the custom function type.
    /// </summary>
    public CustomSqlFunctionType CustomFunctionType { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<CustomSqlFunctionCall>(expression, "expression");
      var replacingExpression = (CustomSqlFunctionCall) expression;
      CustomFunctionType = replacingExpression.CustomFunctionType;
      Arguments.Clear();
      foreach (SqlExpression argument in replacingExpression.Arguments)
        Arguments.Add(argument);
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      var clone = new CustomSqlFunctionCall(CustomFunctionType);
      for (int i = 0, l = Arguments.Count; i < l; i++)
        clone.Arguments.Add((SqlExpression) Arguments[i].Clone(context));
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    public CustomSqlFunctionCall(CustomSqlFunctionType customFunctionType, IEnumerable<SqlExpression> arguments)
      : base(SqlNodeType.CustomFunctionCall)
    {
      CustomFunctionType = customFunctionType;
      Arguments = new Collection<SqlExpression>();
      foreach (SqlExpression argument in arguments)
        Arguments.Add(argument);
    }

    public CustomSqlFunctionCall(CustomSqlFunctionType customFunctionType, params SqlExpression[] arguments)
      : base(SqlNodeType.CustomFunctionCall)
    {
      CustomFunctionType = customFunctionType;
      Arguments = new Collection<SqlExpression>();
      if (arguments!=null)
        foreach (SqlExpression argument in arguments)
          Arguments.Add(argument);
    }
  }
}
