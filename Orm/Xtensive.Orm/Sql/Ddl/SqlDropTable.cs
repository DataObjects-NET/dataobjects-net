// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  public class SqlDropTable : SqlStatement, ISqlCompileUnit
  {
    public Table Table { get; }

    public bool Cascade { get; set; } = true;

    internal override SqlDropTable Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDropTable(t.Table, t.Cascade));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropTable(Table table) : base(SqlNodeType.Drop)
    {
      Table = table;
    }

    internal SqlDropTable(Table table, bool cascade) : base(SqlNodeType.Drop)
    {
      Table = table;
      Cascade = cascade;
    }
  }
}
