// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlVariable>(expression);
      name = replacingExpression.Name;
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlVariable(name, type);

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
