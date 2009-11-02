// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropColumn : SqlAction
  {
    private TableColumn column;
    private bool cascade = true;

    public TableColumn Column {
      get {
        return column;
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

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlDropColumn clone = new SqlDropColumn(column);
      context.NodeMapping[this] = clone;

      return clone;
    }

    internal SqlDropColumn(TableColumn column)
    {
      this.column = column;
    }

    internal SqlDropColumn(TableColumn column, bool cascade)
    {
      this.column = column;
      this.cascade = cascade;
    }
  }
}
