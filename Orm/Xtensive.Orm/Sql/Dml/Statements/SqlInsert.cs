// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlInsert : SqlQueryStatement, ISqlCompileUnit
  {
    private readonly Dictionary<SqlColumn, SqlExpression> values = new Dictionary<SqlColumn, SqlExpression>();

    private SqlTableRef into;
    private SqlSelect from;

    /// <summary>
    /// Gets or sets the table.
    /// </summary>
    /// <value>The table to change.</value>
    public SqlTableRef Into
    {
      get { return into; }
      set { into = value; }
    }

    /// <summary>
    /// Gets the values.
    /// </summary>
    /// <value>The values.</value>
    public Dictionary<SqlColumn, SqlExpression> Values {
      get {
        return values;
      }
    }

    /// <summary>
    /// Gets or sets the FROM clause expression.
    /// </summary>
    public SqlSelect From {
      get {
        return from;
      }
      set {
        from = value;
      }
    }

    /// <inheritdoc />
    internal override SqlInsert Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) => {
        var clone = new SqlInsert();
        clone.Into = t.into?.Clone(c);
        clone.from = t.from?.Clone(c);
        foreach (KeyValuePair<SqlColumn, SqlExpression> p in t.values)
          clone.Values[p.Key.Clone(c)] =
            p.Value is null ? null : p.Value.Clone(c);

        if (t.Hints.Count > 0)
          foreach (var hint in t.Hints)
            clone.Hints.Add(hint.Clone(c));

        return clone;
      });

    // Constructor

    internal SqlInsert(): base(SqlNodeType.Insert)
    {
    }

    internal SqlInsert(SqlTableRef tableRef) : this()
    {
      into = tableRef;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
