// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Dml
{
  [Serializable]
  public class SqlFunctionCall: SqlExpression
  {
    private IList<SqlExpression> arguments;
    private SqlFunctionType functionType;

    /// <summary>
    /// Gets the expressions.
    /// </summary>
    public IList<SqlExpression> Arguments
    {
      get { return arguments; }
    }

    /// <summary>
    /// Gets the function type.
    /// </summary>
    public SqlFunctionType FunctionType
    {
      get { return functionType; }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlFunctionCall>(expression, "expression");
      SqlFunctionCall replacingExpression = expression as SqlFunctionCall;
      functionType = replacingExpression.FunctionType;
      arguments.Clear();
      foreach (SqlExpression argument in replacingExpression.Arguments)
        arguments.Add(argument);
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlFunctionCall clone = new SqlFunctionCall(functionType);
      for (int i = 0, l = arguments.Count; i < l; i++)
        clone.Arguments.Add((SqlExpression)arguments[i].Clone(context));
      context.NodeMapping[this] = clone;
      return clone;
    }

    internal SqlFunctionCall(SqlFunctionType functionType, IEnumerable<SqlExpression> arguments)
      : base(SqlNodeType.FunctionCall)
    {
      this.functionType = functionType;
      this.arguments = new Collection<SqlExpression>();
      foreach (SqlExpression argument in arguments)
        this.arguments.Add(argument);
    }

    internal SqlFunctionCall(SqlFunctionType functionType, params SqlExpression[] arguments)
      : base(SqlNodeType.FunctionCall)
    {
      this.functionType = functionType;
      this.arguments = new Collection<SqlExpression>();
      if (arguments != null)
        foreach (SqlExpression argument in arguments)
          this.arguments.Add(argument);
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}