// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropTable : SqlStatement, ISqlCompileUnit
  {
    private Table table;
    private bool cascade = true;

    public Table Table {
      get {
        return table;
      }
    }

    public bool Cascade {
      get {
        return cascade;
      }
      set {
        cascade = value;
      }
    }

    internal override SqlDropTable Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDropTable(t.table, t.cascade));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropTable(Table table) : base(SqlNodeType.Drop)
    {
      this.table = table;
    }

    internal SqlDropTable(Table table, bool cascade) : base(SqlNodeType.Drop)
    {
      this.table = table;
      this.cascade = cascade;
    }
  }
}
