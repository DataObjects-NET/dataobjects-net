// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Sql.Dml.Collections;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlInsert : SqlQueryStatement, ISqlCompileUnit
  {
    /// <summary>
    /// Gets or sets the table.
    /// </summary>
    /// <value>The table to change.</value>
    public SqlTableRef Into { get; set; }

    /// <summary>
    /// Gets the values.
    /// </summary>
    /// <value>The values.</value>
    [Obsolete("No longer in use. Use ValueRows.Add to add values")]
    public Dictionary<SqlColumn, SqlExpression> Values { get; private set; } = new();

    /// <summary>
    /// Gets rows of values.
    /// </summary>
    public SqlInsertValuesCollection ValueRows { get; private set; } = new SqlInsertValuesCollection();

    /// <summary>
    /// Gets or sets the FROM clause expression.
    /// </summary>
    public SqlSelect From { get; set; }

    internal override SqlInsert Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) => {
        var clone = new SqlInsert {
          Into = t.Into?.Clone(c),
          From = t.From?.Clone(c),
          ValueRows = t.ValueRows.Clone(c)
        };

        if (t.Hints.Count > 0) {
          foreach (SqlHint hint in t.Hints)
            clone.Hints.Add((SqlHint) hint.Clone(c));
        }
        return clone;
      });

    // Constructor

    internal SqlInsert() : base(SqlNodeType.Insert)
    {
    }

    internal SqlInsert(SqlTableRef tableRef) : this()
    {
      Into = tableRef;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
