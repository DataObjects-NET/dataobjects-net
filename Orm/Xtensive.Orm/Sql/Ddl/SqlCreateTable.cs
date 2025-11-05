// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateTable : SqlStatement, ISqlCompileUnit
  {
    public Table Table { get; }

    internal override SqlCreateTable Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCreateTable(t.Table));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreateTable(Table table) : base(SqlNodeType.Create)
    {
      Table = table;
    }
  }
}
