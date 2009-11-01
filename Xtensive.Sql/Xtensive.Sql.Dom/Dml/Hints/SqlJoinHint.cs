// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom.Dml
{
  /// <summary>
  /// Represents a join option (hint) for query optimizer.
  /// </summary>
  /// <remarks>
  /// Note that join hints have different meanings in different RDBMSs: 
  /// join hint for MSSQL should be applied for 2 tables exactly or to SqlJoin object and 
  /// join hint for Oracle could be used with 1 or more tables.
  /// </remarks>
  [Serializable]
  public class SqlJoinHint : SqlHint
  {
    private readonly SqlJoinMethod joinMethod;
    private readonly SqlTable[] sqlTables;
    private readonly SqlJoinExpression sqlJoin;

    /// <summary>
    /// Gets the join method.
    /// </summary>
    /// <value>The join method.</value>
    public SqlJoinMethod JoinMethod
    {
      get { return joinMethod; }
    }

    /// <summary>
    /// Gets the corresponding tables.
    /// </summary>
    /// <value>The tables.</value>
    public SqlTable[] SqlTables
    {
      get { return sqlTables; }
    }

    /// <summary>
    /// Gets the SQL join.
    /// </summary>
    /// <value>The SQL join.</value>
    public SqlJoinExpression SqlJoin
    {
      get { return sqlJoin; }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlJoinHint clone;
      if (sqlJoin!=null) {
        clone = new SqlJoinHint(joinMethod, (SqlJoinExpression)sqlJoin.Clone());
      } else {
        SqlTable[] tables = new SqlTable[sqlTables.Length];
        for (int i = 0, count = sqlTables.Length; i<count; i++) {
          tables[i] = (SqlTable)sqlTables[i].Clone(context);
        }
        clone = new SqlJoinHint(joinMethod, tables);
      }

      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlJoinHint(SqlJoinMethod joinMethod, params SqlTable[] sqlTables)
    {
      this.joinMethod = joinMethod;
      this.sqlTables = sqlTables;
    }

    internal SqlJoinHint(SqlJoinMethod joinMethod, SqlJoinExpression sqlJoin)
    {
      this.joinMethod = joinMethod;
      this.sqlJoin = sqlJoin;
    }

    internal SqlJoinHint(SqlJoinMethod joinMethod)
    {
      this.joinMethod = joinMethod;
    }
  }
}
