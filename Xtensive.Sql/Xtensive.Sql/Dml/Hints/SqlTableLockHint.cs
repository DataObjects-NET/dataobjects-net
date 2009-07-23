// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlTableLockHint : SqlTableHint
  {
    private readonly SqlTableIsolationLevel isolationLevel;
    private readonly SqlTableLockType lockType;

    /// <summary>
    /// Gets the table isolation level.
    /// </summary>
    /// <value>The isolation level.</value>
    public SqlTableIsolationLevel IsolationLevel
    {
      get { return isolationLevel; }
    }

    /// <summary>
    /// Gets the table lock type.
    /// </summary>
    /// <value>The table lock type.</value>
    public SqlTableLockType LockType
    {
      get { return lockType; }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];
      SqlTableLockHint clone = new SqlTableLockHint(Table, isolationLevel, lockType);
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlTableLockHint(SqlTable sqlTable, SqlTableIsolationLevel isolationLevel, SqlTableLockType lockType) : base(sqlTable)
    {
      this.lockType = lockType;
      this.isolationLevel = isolationLevel;
    }
  }
}
