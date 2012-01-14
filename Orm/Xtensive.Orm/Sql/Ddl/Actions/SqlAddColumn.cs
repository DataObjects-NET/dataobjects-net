// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlAddColumn : SqlAction
  {
    public TableColumn Column { get; private set; }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      var clone = new SqlAddColumn(Column);
      context.NodeMapping[this] = clone;

      return clone;
    }

    // Constructors

    internal SqlAddColumn(TableColumn column)
    {
      Column = column;
    }
  }
}
