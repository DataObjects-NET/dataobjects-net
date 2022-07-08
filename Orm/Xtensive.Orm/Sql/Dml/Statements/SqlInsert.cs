// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

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

    internal override SqlInsert Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) => {
        SqlInsert clone = new SqlInsert();
        if (t.Into!=null)
          clone.Into = t.Into.Clone(c);
        if (t.from!=null)
          clone.From = t.from.Clone(c);
        foreach (KeyValuePair<SqlColumn, SqlExpression> p in t.values)
          clone.Values[(SqlTableColumn) p.Key.Clone(c)] =
            p.Value?.Clone(c);

        if (t.Hints.Count > 0)
          foreach (SqlHint hint in t.Hints)
            clone.Hints.Add((SqlHint) hint.Clone(c));

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
