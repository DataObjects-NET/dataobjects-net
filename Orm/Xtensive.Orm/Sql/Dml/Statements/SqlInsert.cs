// Copyright (C) 2003-2023 Xtensive LLC.
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

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.TryGetValue(this, out var value)) {
        return value;
      }

      SqlInsert clone = new SqlInsert();
      clone.Into = (SqlTableRef) Into?.Clone(context);
      clone.From = (SqlSelect) From?.Clone(context);
#pragma warning disable CS0618 // Type or member is obsolete
      //remove cloning after changing code.
      foreach (KeyValuePair<SqlColumn, SqlExpression> p in Values)
        clone.Values[(SqlTableColumn) p.Key.Clone(context)] =
          p.Value.IsNullReference() ? null : (SqlExpression) p.Value.Clone(context);
#pragma warning restore CS0618 // Type or member is obsolete
      clone.ValueRows = ValueRows.Clone(context);

      if (Hints.Count > 0)
        foreach (SqlHint hint in Hints)
          clone.Hints.Add((SqlHint) hint.Clone(context));

      context.NodeMapping[this] = clone;
      return clone;
    }

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
