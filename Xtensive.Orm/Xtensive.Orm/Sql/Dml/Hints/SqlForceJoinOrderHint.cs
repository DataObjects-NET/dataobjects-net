// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Linq;
using System.Collections.Generic;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlForceJoinOrderHint : SqlHint
  {
    private SqlTable[] tables;

    /// <summary>
    /// Gets the corresponding tables.
    /// </summary>
    public IEnumerable<SqlTable> Tables { get { return tables; } }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];
      var clone = new SqlForceJoinOrderHint();
      if (tables!=null)
        clone.tables = tables
          .Select(table => (SqlTable) table.Clone())
          .ToArray();
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlForceJoinOrderHint()
    {
    }

    internal SqlForceJoinOrderHint(SqlTable[] tables)
    {
      this.tables = tables;
    }
  }
}
