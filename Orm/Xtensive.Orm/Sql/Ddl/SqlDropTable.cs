// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropTable : SqlStatement, ISqlCompileUnit
  {
    private bool cascade = true;

    public Table Table { get; }

    public bool Cascade {
      get {
        return cascade;
      }
      set {
        cascade = value;
      }
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlDropTable(Table, cascade);

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
      this.cascade = cascade;
    }
  }
}
