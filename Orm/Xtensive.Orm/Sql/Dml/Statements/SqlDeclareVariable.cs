// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlDeclareVariable : SqlStatement, ISqlCompileUnit
  {
    /// <summary>
    /// Gets the variable.
    /// </summary>
    /// <value>The variable.</value>
    public SqlVariable Variable { get; }

    internal override SqlDeclareVariable Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDeclareVariable(t.Variable));


    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDeclareVariable(SqlVariable variable)
      : base(SqlNodeType.DeclareVariable)
    {
      Variable = variable;
    }
  }
}
