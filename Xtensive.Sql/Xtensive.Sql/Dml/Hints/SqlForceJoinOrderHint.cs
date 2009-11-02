// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlForceJoinOrderHint : SqlHint
  {
    private SqlTable[] sqlTables;

    /// <summary>
    /// Gets the corresponding tables.
    /// </summary>
    /// <value>The tables.</value>
    public SqlTable[] SqlTables
    {
      get { return sqlTables; }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlForceJoinOrderHint clone;
      SqlTable[] tables = new SqlTable[sqlTables.Length];
      for (int i = 0, count = sqlTables.Length; i<count; i++) {
        tables[i] = (SqlTable)sqlTables[i].Clone(context);
      }
      clone = new SqlForceJoinOrderHint(tables);

      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlForceJoinOrderHint(SqlTable[] sqlTables) : this()
    {
      this.sqlTables = sqlTables;
    }

    internal SqlForceJoinOrderHint()
    {
    }
  }
}
