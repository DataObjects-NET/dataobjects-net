// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Sql.Dom.Ddl
{
  [Serializable]
  public class SqlDropDefault : SqlAction
  {
    private TableColumn column;

    public TableColumn Column {
      get {
        return column;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlDropDefault clone = new SqlDropDefault(column);
      context.NodeMapping[this] = clone;

      return clone;
    }

    internal SqlDropDefault(TableColumn column)
    {
      this.column = column;
    }
  }
}
