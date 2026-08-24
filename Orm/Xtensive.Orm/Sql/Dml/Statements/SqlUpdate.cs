// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dml
{
  public class SqlUpdate : SqlQueryStatement, ISqlCompileUnit
  {
    private readonly Dictionary<ISqlLValue, SqlExpression> values = new();
    private SqlExpression where;

    /// <summary>
    /// Gets or sets the table.
    /// </summary>
    /// <value>The table to change.</value>
    public SqlTableRef Update { get; set; }

    /// <summary>
    /// Gets the values.
    /// </summary>
    /// <value>The values.</value>
    public Dictionary<ISqlLValue, SqlExpression> Values => values;

    /// <summary>
    /// Gets or sets the WHERE clause expression.
    /// </summary>
    /// <value>The WHERE clause expression.</value>
    public SqlExpression Where {
      get { return where; }
      set {
        if (value is not null && value.GetType()!=typeof(SqlCursor))
          SqlValidator.EnsureIsBooleanExpression(value);
        where = value;
      }
    }

    /// <summary>
    /// Gets or sets the FROM clause expression.
    /// </summary>
    public SqlTable From { get; set; }

    /// <summary>
    /// Gets or sets the LIMIT clause expression.
    /// </summary>
    public SqlExpression Limit { get; set; }

    internal override SqlUpdate Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) => {
        var clone = new SqlUpdate();
        if (t.Update != null)
          clone.Update = t.Update.Clone(c);
        if (t.From != null)
          clone.From = (SqlQueryRef) t.From.Clone(c);
        foreach (KeyValuePair<ISqlLValue, SqlExpression> p in t.values)
          clone.Values[(ISqlLValue) ((SqlExpression) p.Key).Clone(c)] =
            p.Value?.Clone(c);
        if (t.where is not null)
          clone.Where = t.where.Clone(c);
        if (t.Limit is not null)
          clone.Limit = t.where.Clone(c);
        if (t.Hints.Count > 0)
          foreach (SqlHint hint in t.Hints)
            clone.Hints.Add((SqlHint) hint.Clone(c));

        return clone;
      });

    // Constructor

    internal SqlUpdate(): base(SqlNodeType.Update)
    {
    }

    internal SqlUpdate(SqlTableRef table) : this()
    {
      Update = table;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
