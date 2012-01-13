// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlVariable : SqlExpression, ISqlCursorFetchTarget
  {
    private string name;
    private readonly SqlValueType type;

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name 
    {
      get { return name;}
    }

    /// <summary>
    /// Gets the type.
    /// </summary>
    /// <value>The type.</value>
    public SqlValueType Type 
    {
      get { return type; }
    }

    public SqlDeclareVariable Declare()
    {
      return new SqlDeclareVariable(this);
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlVariable>(expression, "expression");
      SqlVariable replacingExpression = expression as SqlVariable;
      name = replacingExpression.Name;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];
      
      SqlVariable clone = new SqlVariable(name, type);
      context.NodeMapping[this] = clone;
      return clone;
    }

    internal SqlVariable(string name, SqlValueType type)
      : base(SqlNodeType.Variable)
    {
      this.name = name;
      this.type = type;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
