// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropDefault : SqlAction
  {
    public TableColumn Column { get; private set; }

    internal override SqlDropDefault Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDropDefault(t.Column));

    // Constructors

    internal SqlDropDefault(TableColumn column)
    {
      Column = column;
    }
  }
}
