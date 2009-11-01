// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Ddl
{
  [Serializable]
  public class SqlSetDefault : SqlAction
  {
    private TableColumn column;
    private SqlExpression defaultValue;

    public TableColumn Column {
      get {
        return column;
      }
    }

    public SqlExpression DefaultValue {
      get {
        return defaultValue;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlSetDefault clone = new SqlSetDefault((SqlExpression)defaultValue.Clone(context), column);
      context.NodeMapping[this] = clone;

      return clone;
    }

    internal SqlSetDefault(SqlExpression defaultValue, TableColumn column)
    {
      this.defaultValue = defaultValue;
      this.column = column;
    }
  }
}
