// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Sql.Dom.Ddl
{
  [Serializable]
  public class SqlAddColumn : SqlAction
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

      SqlAddColumn clone = new SqlAddColumn(column);
      context.NodeMapping[this] = clone;

      return clone;
    }

    internal SqlAddColumn(TableColumn column)
    {
      this.column = column;
    }
  }
}
