// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlDeclareVariable : SqlStatement, ISqlCompileUnit
  {
    private SqlVariable variable;

    /// <summary>
    /// Gets the variable.
    /// </summary>
    /// <value>The variable.</value>
    public SqlVariable Variable
    {
      get { return variable; }
    }

    internal override SqlDeclareVariable Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDeclareVariable(t.variable));

    internal SqlDeclareVariable(SqlVariable variable)
      : base(SqlNodeType.DeclareVariable)
    {
      this.variable = variable;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
