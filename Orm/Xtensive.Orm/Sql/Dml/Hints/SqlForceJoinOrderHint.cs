// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
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

    internal override SqlForceJoinOrderHint Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) => {
        if (t.tables is null)
          return new SqlForceJoinOrderHint();
        var source = t.tables;
        var tablesClone = new SqlTable[source.Length];
        for (int i = 0; i < source.Length; i++)
          tablesClone[i] = source[i].Clone(c);
        return new SqlForceJoinOrderHint(tablesClone);
      });

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
