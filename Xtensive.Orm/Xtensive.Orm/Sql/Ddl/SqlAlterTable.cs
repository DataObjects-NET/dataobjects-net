// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlAlterTable : SqlStatement, ISqlCompileUnit
  {
    private SqlAction action;
    private Table table;

    public SqlAction Action {
      get {
        return action;
      }
    }

    public Table Table {
      get {
        return table;
      }
    }


    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlAlterTable clone = new SqlAlterTable(table, (SqlAction)action.Clone(context));
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlAlterTable(Table table, SqlAction action)
      : base(SqlNodeType.Alter)
    {
      this.action = action;
      this.table = table;
    }
  }
}
