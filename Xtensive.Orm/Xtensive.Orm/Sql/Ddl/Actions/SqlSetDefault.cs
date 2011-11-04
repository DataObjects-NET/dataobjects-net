// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlSetDefault : SqlAction
  {
    public TableColumn Column { get; private set; }
    public SqlExpression DefaultValue { get; private set; }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      var clone = new SqlSetDefault((SqlExpression) DefaultValue.Clone(context), Column);
      context.NodeMapping[this] = clone;

      return clone;
    }

    internal SqlSetDefault(SqlExpression defaultValue, TableColumn column)
    {
      DefaultValue = defaultValue;
      Column = column;
    }
  }
}
