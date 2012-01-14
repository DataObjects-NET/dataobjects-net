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
  public class SqlUserFunctionCall : SqlFunctionCall
  {
    private string name;

    /// <summary>
    /// Gets the function name.
    /// </summary>
    public string Name {
      get {
        return name;
      }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlUserFunctionCall>(expression, "expression");
      SqlUserFunctionCall replacingExpression = expression as SqlUserFunctionCall;
      name = replacingExpression.Name;
      Arguments.Clear();
      foreach (SqlExpression argument in replacingExpression.Arguments)
        Arguments.Add(argument);
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];
      
      SqlUserFunctionCall clone = new SqlUserFunctionCall(name);
      for (int i = 0, l = Arguments.Count; i < l; i++)
        clone.Arguments.Add((SqlExpression)Arguments[i].Clone(context));
      context.NodeMapping[this] = clone;
      return clone;
    }

    internal SqlUserFunctionCall(string name, IEnumerable<SqlExpression> arguments)
      : base(SqlFunctionType.UserDefined, arguments)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      this.name = name;
    }

    internal SqlUserFunctionCall(string name, params SqlExpression[] arguments)
      : base(SqlFunctionType.UserDefined, arguments)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      this.name = name;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}