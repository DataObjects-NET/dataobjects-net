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

    internal override SqlSetDefault Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlSetDefault(t.DefaultValue.Clone(c), t.Column));

    internal SqlSetDefault(SqlExpression defaultValue, TableColumn column)
    {
      DefaultValue = defaultValue;
      Column = column;
    }
  }
}
