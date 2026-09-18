// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  public class SqlVariable : SqlExpression, ISqlCursorFetchTarget
  {
    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the type.
    /// </summary>
    /// <value>The type.</value>
    public SqlValueType Type { get; }

    public SqlDeclareVariable Declare() => new SqlDeclareVariable(this);

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlVariable>(expression);
      Name = replacingExpression.Name;
    }

    internal override SqlVariable Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlVariable(t.Name, t.Type));

    internal SqlVariable(string name, SqlValueType type)
      : base(SqlNodeType.Variable)
    {
      Name = name;
      Type = type;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
