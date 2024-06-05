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

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlDeclareVariable(Variable);

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
